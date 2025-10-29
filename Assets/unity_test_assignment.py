#!/usr/bin/env python3
"""
events_gen.py — simple NDJSON game-event generator.

Emits a stream of JSON lines with timestamps that simulate a small 2D skirmish.
Event types: entity_spawned, entity_moved, entity_attacked, entity_damaged, entity_died.

Usage examples:
  python events_gen.py --out events.jsonl --duration 25
  python events_gen.py --stream        # print to stdout in real time
"""
import argparse
import json
import math
import random
import sys
import time
from dataclasses import dataclass, field
from typing import Dict

# ---------- Model ----------

@dataclass
class Entity:
    id: str
    team: str
    x: float
    y: float
    hp: int = 100
    max_hp: int = 100
    alive: bool = True
    _tx: float = field(default=0.0)  # target x
    _ty: float = field(default=0.0)  # target y

# ---------- Generator ----------

class Skirmish:
    # Constants for game balance
    ATTACK_RANGE = 5.0
    MOVE_SPEED = 2.0
    MIN_MOVE_DISTANCE = 0.2

    def __init__(self, rng: random.Random, arena_size: float = 20.0):
        self.rng = rng
        self.entities: Dict[str, Entity] = {}
        self.time_ms = 0
        self.arena = arena_size

    def _emit(self, out, ev_type: str, eid: str, data: dict):
        row = {
            "ts": self.time_ms,                # milliseconds since start
            "type": ev_type,                   # "entity_spawned" | ...
            "id": eid,                         # entity id (string)
            "data": data                       # event payload
        }
        out.write(json.dumps(row, ensure_ascii=False) + "\n")

    def seed_entities(self, out, per_team: int = 3):
        def rand_pos(side: str):
            margin = 2.0
            if side == "A":
                return (self.rng.uniform(margin, self.arena * 0.35),
                        self.rng.uniform(margin, self.arena - margin))
            return (self.rng.uniform(self.arena * 0.65, self.arena - margin),
                    self.rng.uniform(margin, self.arena - margin))

        for team in ("A", "B"):
            for i in range(per_team):
                eid = f"{team}{i+1}"
                x, y = rand_pos(team)
                e = Entity(id=eid, team=team, x=x, y=y)
                e._tx, e._ty = self._random_target()
                self.entities[eid] = e
                self._emit(out, "entity_spawned", eid, {
                    "x": round(e.x, 2),
                    "y": round(e.y, 2),
                    "hp": e.hp,
                    "max_hp": e.max_hp,
                    "team": e.team,
                    "name": f"Unit {eid}"
                })

    def _random_target(self):
        margin = 1.0
        return (self.rng.uniform(margin, self.arena - margin),
                self.rng.uniform(margin, self.arena - margin))

    def _find_target_in_range(self, attacker: Entity):
        """Find the closest enemy within attack range."""
        targets = [e for e in self.entities.values()
                   if e.alive and e.team != attacker.team]
        if not targets:
            return None

        closest = None
        min_dist = self.ATTACK_RANGE
        for t in targets:
            dist = math.hypot(t.x - attacker.x, t.y - attacker.y)
            if dist <= min_dist:
                min_dist = dist
                closest = t
        return closest

    def _step_moves(self, out, dt_ms: int):
        # Move each living entity toward enemy or random target
        dt = dt_ms / 1000.0
        for e in list(self.entities.values()):
            if not e.alive:
                continue

            # Check if there's an enemy in attack range
            target_in_range = self._find_target_in_range(e)
            if target_in_range:
                # Don't move if we can attack
                continue

            # Find closest enemy to move toward
            enemies = [en for en in self.entities.values()
                      if en.alive and en.team != e.team]
            if enemies:
                closest_enemy = min(enemies,
                                  key=lambda en: math.hypot(en.x - e.x, en.y - e.y))
                e._tx, e._ty = closest_enemy.x, closest_enemy.y
            else:
                # No enemies, use random target
                dx = e._tx - e.x
                dy = e._ty - e.y
                dist = math.hypot(dx, dy)
                if dist < self.MIN_MOVE_DISTANCE:
                    e._tx, e._ty = self._random_target()

            # Move toward target
            dx = e._tx - e.x
            dy = e._ty - e.y
            dist = math.hypot(dx, dy)
            if dist > self.MIN_MOVE_DISTANCE:
                step = self.MOVE_SPEED * dt
                nx = e.x + (dx / dist) * step
                ny = e.y + (dy / dist) * step
                # clamp to arena
                nx = max(0.5, min(self.arena - 0.5, nx))
                ny = max(0.5, min(self.arena - 0.5, ny))
                e.x, e.y = nx, ny
                self._emit(out, "entity_moved", e.id, {
                    "x": round(e.x, 2),
                    "y": round(e.y, 2)
                })

    def _step_combat(self, out):
        # Each entity attacks if it has a target in range
        # Process attacks sequentially, checking if attacker is still alive
        deaths = []

        alive_entities = [e for e in self.entities.values() if e.alive]
        for atk in alive_entities:
            # Skip if this entity died earlier in this combat phase
            if not atk.alive:
                continue

            tgt = self._find_target_in_range(atk)
            if not tgt:
                continue

            self._emit(out, "entity_attacked", atk.id, {"target_id": tgt.id})
            dmg = self.rng.randint(8, 16)
            tgt.hp = max(0, tgt.hp - dmg)
            self._emit(out, "entity_damaged", tgt.id, {"amount": dmg, "hp": tgt.hp})

            # Mark entity as dead immediately so it won't attack later in this phase
            if tgt.hp == 0 and tgt.alive:
                tgt.alive = False
                deaths.append((tgt, atk))

        # Emit all death events after all attacks/damage are processed
        for tgt, atk in deaths:
            self._emit(out, "entity_died", tgt.id, {"killer_id": atk.id})

    def run(self, out, duration_s: int, tick_ms: int, realtime: bool = False):
        # Initial spawn at t=0
        self.seed_entities(out)
        last_combat_s = -1
        steps = int(duration_s * 1000 / tick_ms)
        t0 = time.perf_counter()
        for _ in range(steps):
            self.time_ms += tick_ms
            self._step_moves(out, tick_ms)
            # One combat check per 1000 ms
            if self.time_ms // 1000 != last_combat_s:
                last_combat_s = self.time_ms // 1000
                self._step_combat(out)
            if realtime:
                # Sleep to match the tick rate
                elapsed_ms = int((time.perf_counter() - t0) * 1000)
                ahead = self.time_ms - elapsed_ms
                if ahead > 0:
                    time.sleep(ahead / 1000.0)

# ---------- CLI ----------

def main():
    ap = argparse.ArgumentParser(description="NDJSON game-event generator")
    ap.add_argument("--out", type=str, default="", help="Output file path. Omit for stdout.")
    ap.add_argument("--duration", type=int, default=25, help="Duration in seconds to simulate.")
    ap.add_argument("--tick", type=int, default=200, help="Tick in milliseconds between move events.")
    ap.add_argument("--seed", type=int, default=42, help="Random seed for reproducibility.")
    ap.add_argument("--stream", action="store_true", help="Emit in real time and flush each line.")
    args = ap.parse_args()

    rng = random.Random(args.seed)
    skirmish = Skirmish(rng)

    out = sys.stdout if not args.out else open(args.out, "w", encoding="utf-8")
    try:
        if args.stream:
            # Wrap stdout with immediate flush behavior
            class _Writer:
                def write(self, s):
                    sys.stdout.write(s)
                    sys.stdout.flush()
            writer = _Writer()
            skirmish.run(writer, args.duration, args.tick, realtime=True)
        else:
            skirmish.run(out, args.duration, args.tick, realtime=False)
    finally:
        if out is not sys.stdout:
            out.close()

if __name__ == "__main__":
    main()
