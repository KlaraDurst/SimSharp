#region License Information
/*
 * This file is part of SimSharp which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using SimSharp.Visualization.Push.Shapes;
using SimSharp.Visualization.Push;
using SimSharp.Visualization;

namespace SimSharp.Samples {
  public class GasStationRefuelingPushAnimation {
    /*
     * Gas Station Refueling example
     *
     * Covers:
     *  - Resources: Resource
     *  - Resources: Container
     *  - Waiting for other processes
     *
     * Scenario:
     *  A gas station has a limited number of gas pumps that share a common
     *  fuel reservoir. Cars randomly arrive at the gas station, request one
     *  of the fuel pumps and start refueling from that reservoir.
     *
     *  A gas station control process observes the gas station's fuel level
     *  and calls a tank truck for refueling if the station's level drops
     *  below a threshold.
     */
    private const int RandomSeed = 42;
    private const int GasStationSize = 200; // liters
    private const int Threshold = 10; // Threshold for calling the tank truck (in %)
    private const int FuelTankSize = 50; // liters
    private const int MinFuelTankLevel = 5; // Min levels of fuel tanks (in liters)
    private const int MaxFuelTankLevel = 25; // Max levels of fuel tanks (in liters)
    private const int RefuelingSpeed = 2; // liters / second
    private static readonly TimeSpan TankTruckTime = TimeSpan.FromMinutes(10); // Minutes it takes the tank truck to arrive
    private static readonly TimeSpan MinTInter = TimeSpan.FromMinutes(5); // Create a car every min seconds
    private static readonly TimeSpan MaxTInter = TimeSpan.FromMinutes(50); // Create a car every max seconds
    private static readonly TimeSpan SimTime = TimeSpan.FromMinutes(150); // Simulation time

    private static readonly int CarHeight = 50; // Height of car rectangles

    private IEnumerable<Event> Car(string name, Simulation env, Resource gasStation, Container fuelPump) {
      /*
       * A car arrives at the gas station for refueling.
       * 
       * It requests one of the gas station's fuel pumps and tries to get the
       * desired amount of gas from it. If the stations reservoir is
       * depleted, the car has to wait for the tank truck to arrive.
       */
      var fuelTankLevel = env.RandUniform(MinFuelTankLevel, MaxFuelTankLevel + 1);
      env.Log("{0} arriving at gas station at {1}", name, env.Now);
      using (var req = gasStation.Request()) {
        var start = env.Now;
        // Request one of the gas pumps
        yield return req;

        // Get the required amount of fuel
        var litersRequired = FuelTankSize - fuelTankLevel;

        // Car visualization (at gas station)
        Rect fullCarRect = new Rect(gasStation.InUse < 2 ? 275 : 475, 250, Convert.ToInt32(litersRequired), CarHeight);
        Rect emptyCarRect = new Rect(fullCarRect.X, fullCarRect.Y, 0, CarHeight);
        Animation carAnimation = env.AnimationBuilder.Animate(name, fullCarRect, fullCarRect, env.Now, env.Now, "none", "yellow", 1, true);

        if (litersRequired > fuelPump.Level) {
          var level = fuelPump.Level;
          var firstRefuelDuration = TimeSpan.FromSeconds(level / RefuelingSpeed);
          var secondRefuelDuration = TimeSpan.FromSeconds((litersRequired - level) / RefuelingSpeed);
          yield return fuelPump.Get(level); // draw it empty

          // First car tank fill visualization
          Rect tempCarRect = new Rect(emptyCarRect.X, emptyCarRect.Y, Convert.ToInt32(level), CarHeight);
          Animation fillCarAnimation = env.AnimationBuilder.Animate(name+"Tank", emptyCarRect, tempCarRect, env.Now, env.Now + firstRefuelDuration, "yellow", "yellow", 1, true);

          yield return env.Timeout(firstRefuelDuration);
          yield return fuelPump.Get(litersRequired - level); // wait for the rest

          // Second car tank fill visualization
          fillCarAnimation.Update(tempCarRect, fullCarRect, env.Now, env.Now + secondRefuelDuration, "yellow", "yellow", 1, false);
          carAnimation.Update(fullCarRect, fullCarRect, env.Now, env.Now + secondRefuelDuration, "none", "yellow", 1, false); 

          yield return env.Timeout(secondRefuelDuration);
        } else {
          var refuelDuration = TimeSpan.FromSeconds(litersRequired / RefuelingSpeed);
          yield return fuelPump.Get(litersRequired);

          // Car tank fill visualization
          env.AnimationBuilder.Animate(name+"Tank", emptyCarRect, fullCarRect, env.Now, env.Now + refuelDuration, "yellow", "yellow", 1, false);
          carAnimation.Update(fullCarRect, fullCarRect, env.Now, env.Now + refuelDuration, "none", "yellow", 1, false);

          yield return env.Timeout(refuelDuration);
        }
        env.Log("{0} finished refueling in {1} seconds.", name, (env.Now - start).TotalSeconds);
      }
    }


    private IEnumerable<Event> GasStationControl(Simulation env, Container fuelPump) {
      /*
       * Call the tank truck if the level falls below a threshold.
       */
      int i = -1;
      while (true) {
        yield return fuelPump.WhenAtMost(fuelPump.Capacity * (Threshold / 100.0));
        i++;
        // We need to call the tank truck now!
        env.Log("Calling tank truck at {0}", env.Now);
        // Wait for the tank truck to arrive and refuel the station
        yield return env.Process(TankTruck("Truck " + i, env, fuelPump));
      }
    }

    private IEnumerable<Event> GasStationVisualization(Simulation env, Container fuelPump) {
      /*
       * Update the visualization of the fuel pump if the level changes 
       */
      Rect fullFuelPumpRect = new Rect(275, 550, 250, GasStationSize);
      Animation fuelPumpAnimation = env.AnimationBuilder.Animate("fuelPumpTank", fullFuelPumpRect, fullFuelPumpRect, env.Now, env.Now, "black", "black", 1, true);

      while (true) {
        yield return fuelPump.WhenChange();
        // Visualization has to be updated
        Rect currFuelPumpRect = (Rect) fuelPumpAnimation.GetShape0();
        Rect levelFuelPumpRect = new Rect(currFuelPumpRect.X, Convert.ToInt32(currFuelPumpRect.Y + currFuelPumpRect.Height - fuelPump.Level), 250, Convert.ToInt32(fuelPump.Level));
        fuelPumpAnimation.Update(currFuelPumpRect, levelFuelPumpRect, env.Now, env.Now, "black", "black", 1, true);
      }
    }

    private IEnumerable<Event> TankTruck(string name, Simulation env, Container fuelPump) {
      // Arrives at the gas station after a certain delay and refuels it.
      yield return env.Timeout(TankTruckTime);
      env.Log("Tank truck arriving at time {0}", env.Now);

      // Tank truck visualization
      Rect truckRect = new Rect(575, 550, 50, 100);
      Animation truckAnimation = env.AnimationBuilder.Animate(name, truckRect, truckRect, env.Now, env.Now, "blue", "blue", 1, true);

      var amount = fuelPump.Capacity - fuelPump.Level;
      yield return fuelPump.Put(amount);
      env.Log("Tank truck finished refuelling {0} liters at time {1}.", amount, env.Now);

      // Remove tank truck visualization
      truckAnimation.Update(truckRect, truckRect, env.Now, env.Now, "blue", "blue", 1, false);
    }

    private IEnumerable<Event> CarGenerator(Simulation env, Resource gasStation, Container fuelPump) {
      // Generate new cars that arrive at the gas station.
      var i = 0;
      while (true) {
        i++;
        yield return env.Timeout(env.RandUniform(MinTInter, MaxTInter));
        env.Process(Car("Car " + i, env, gasStation, fuelPump));
      }
    }

    public void Simulate(int rseed = RandomSeed) {
      // Setup and start the simulation
      // Create environment and start processes
      var env = new Simulation(DateTime.Now.Date, rseed, new AnimationBuilder(1000, 1000, "Gas Station Refueling", 1));
      env.Log("== Gas Station refuelling push animation ==");
      var gasStation = new Resource(env, 2) {
        QueueLength = new TimeSeriesMonitor(env, name: "Waiting cars", collect: true),
        WaitingTime = new SampleMonitor(name: "Waiting time", collect: true),
        Utilization = new TimeSeriesMonitor(env, name: "Station utilization"),
      };

      // BuildAnimation has to be turned on before first Animation is created
      env.AnimationBuilder.DebugAnimation = false;
      env.AnimationBuilder.EnableAnimation = true;

      // Gas station visualization
      Rect gasStationRectLeft = new Rect(275, 350, 50, 100);
      Rect gasStationRectRight = new Rect(475, 350, 50, 100);
      env.AnimationBuilder.Animate("gasStationLeft", gasStationRectLeft, gasStationRectLeft, env.StartDate, env.StartDate, "grey", "grey", 1, true);
      env.AnimationBuilder.Animate("gasStationRight", gasStationRectRight, gasStationRectRight, env.StartDate, env.StartDate, "grey", "grey", 1, true);

      var fuelPump = new Container(env, GasStationSize, GasStationSize) {
        Fillrate = new TimeSeriesMonitor(env, name: "Tank fill rate")
      };

      // Fuel pump visualization
      Rect fuelPumpRect = new Rect(275, 550, 250, GasStationSize);
      env.AnimationBuilder.Animate("fuelPump", fuelPumpRect, fuelPumpRect, env.StartDate, env.StartDate, "none", "black", 1, true);

      env.Process(GasStationControl(env, fuelPump));
      env.Process(GasStationVisualization(env, fuelPump));
      env.Process(CarGenerator(env, gasStation, fuelPump));

      // Execute!
      var watch = new System.Diagnostics.Stopwatch();

      watch.Start();

      env.Run(SimTime);

      watch.Stop();

      Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
      //env.Log(gasStation.QueueLength.Summarize());
      //env.Log(gasStation.WaitingTime.Summarize());
      //env.Log(gasStation.Utilization.Summarize());
      //env.Log(fuelPump.Fillrate.Summarize());
    }
  }
}
