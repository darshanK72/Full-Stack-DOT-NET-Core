---
module: 02. Object Oriented Programming
difficulty: Hard
chapters: 05 Inheritance, 05 Operator Overloading, 05 Polymorphism, 09 Vehicle Example
domain: FleetManagement
---

# Fleet Vehicle Manager

Build a **.NET 8 console application from scratch** managing a fleet hierarchy with polymorphic dispatch and custom distance arithmetic.

## Business context

Logistics tracks cars and trucks. Dispatch assigns routes; reporting sums odometer-style distances using a value type wrapper.

## Definitions

**Struct `Distance`** (preview: struct for small immutable value)

- `Kilometers` (double, ≥ 0)
- Constructor validates non-negative
- Override `ToString()` → `"{Kilometers:F1} km"`
- Overload `+` and `-` returning new `Distance`; `-` throws if result would be negative

**Abstract class `Vehicle`**

- `PlateNumber` (string), `Make` (string)
- Protected accumulated `_distance` (`Distance`, starts at 0 km)
- `public Distance TotalDistance => _distance`
- Abstract `string VehicleKind { get; }`
- Virtual `void Drive(Distance delta)` — adds to `_distance`
- Virtual `string Describe()` → `"{VehicleKind}: {PlateNumber} ({Make}) — {TotalDistance}"`

**`Car : Vehicle`**

- `PassengerCapacity` (int ≥ 1)
- `VehicleKind` → `"Car"`
- Override `Describe()` to append capacity

**`Truck : Vehicle`**

- `LoadCapacityTons` (double > 0)
- `VehicleKind` → `"Truck"`
- Override `Drive` — reject delta > 500 km per trip (`ArgumentOutOfRangeException`); else call `base.Drive`

**Class `FleetManager`**

- `AddVehicle(Vehicle v)` — false if duplicate `PlateNumber` (case-insensitive)
- `List<Vehicle> GetByKind(string kind)` — case-insensitive match on `VehicleKind`
- `Distance GetFleetTotalDistance()` — sum each vehicle's `TotalDistance` using overloaded `+`

## Demo Main

Add car and truck, drive each, print descriptions and fleet total.

## Constraints

- net8, explicit usings
- Use `virtual`/`override`/`abstract` appropriately

## Non-goals

Fuel cost, GPS, persistence

## Evaluation

[EVALUATION.md](EVALUATION.md)
