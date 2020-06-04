#region License Information
/*
 * This file is part of SimSharp which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using SimSharp.Visualization;
using SimSharp.Visualization.Advanced;
using SimSharp.Visualization.Advanced.AdvancedShapes;
using SimSharp.Visualization.Advanced.AdvancedStyles;
using SimSharp.Visualization.Basic.Resources;
using SimSharp.Visualization.Basic.Shapes;
using SimSharp.Visualization.Basic.Styles;
using SimSharp.Visualization.Player;

namespace SimSharp.Samples {
  public class GasStationRefuelingAdvAnimation {
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

    //private static readonly TimeSpan TankTruckTime = TimeSpan.FromMinutes(1); // Minutes it takes the tank truck to arrive
    //private static readonly TimeSpan MinTInter = TimeSpan.FromSeconds(10); // Create a car every min seconds
    //private static readonly TimeSpan MaxTInter = TimeSpan.FromSeconds(20); // Create a car every max seconds
    //private static readonly TimeSpan SimTime = TimeSpan.FromMinutes(3); // Simulation time

    private static readonly int CarHeight = 50; // Height of car rectangles
    private static bool[] gasStations = { true, true };
    private static AnimationUtil util;

    private int GetFreeGasStation() {
      if (gasStations[0]) {
        gasStations[0] = false;
        return 1;
      } else {
        gasStations[1] = false;
        return 2;
      }
    }

    private void FreeGasStation(int i) {
      gasStations[i - 1] = true;
    }

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
        Process thisProcess = env.ActiveProcess;
        AdvancedStyle carStyle = new AdvancedStyle("none", "green", 1);
        AdvancedStyle carTankStyle = new AdvancedStyle("green", "green", 1);
        AdvancedRect carRect = new AdvancedRect(GetFreeGasStation() == 1 ? 275 : 475, 250, Convert.ToInt32(litersRequired), CarHeight);
        AdvancedAnimation fullCarAnimation = env.AnimationBuilder.Animate(
          name,
          carRect,
          carStyle,
          (Func<int, bool>)(t => gasStation.UsedBy(thisProcess)));

        if (litersRequired > fuelPump.Level && fuelPump.Level > 0) {
          var level = fuelPump.Level;
          var firstRefuelDuration = TimeSpan.FromSeconds(level / RefuelingSpeed);
          var secondRefuelDuration = TimeSpan.FromSeconds((litersRequired - level) / RefuelingSpeed);
          yield return fuelPump.Get(level); // draw it empty

          // First car tank fill visualization
          DateTime refuelStartTime = env.Now;
          DateTime refuelPauseTime = env.Now + firstRefuelDuration;
          int entryFrame = Convert.ToInt32((refuelStartTime - env.StartDate).TotalSeconds * env.AnimationBuilder.FPS) + 1;
          int pauseFrame = Convert.ToInt32((refuelPauseTime - env.StartDate).TotalSeconds * env.AnimationBuilder.FPS);
          int firstRefuelFrames = pauseFrame - entryFrame + 1;

          AdvancedRect carTankRect = new AdvancedRect(
            carRect.X,
            250,
            util.GetIntValueAt(refuelStartTime, refuelPauseTime, 0, Convert.ToInt32(level)),
            CarHeight);

          AdvancedAnimation tempCarAnimation = env.AnimationBuilder.Animate(
            name + "Tank",
            carTankRect,
            carTankStyle,
            (Func<int, bool>) (t => gasStation.UsedBy(thisProcess)));

          yield return env.Timeout(firstRefuelDuration);
          yield return fuelPump.Get(litersRequired - level); // wait for the rest

          // Second car tank fill visualization
          DateTime refuelContinueTime = env.Now;
          DateTime refuelEndTime = env.Now + secondRefuelDuration;
          int continueFrame = Convert.ToInt32((refuelContinueTime - env.StartDate).TotalSeconds * env.AnimationBuilder.FPS) + 1;
          int exitFrame = Convert.ToInt32((refuelEndTime - env.StartDate).TotalSeconds * env.AnimationBuilder.FPS);
          int secondRefuelFrames = exitFrame - continueFrame + 1;

          carTankRect.Width = util.GetIntValueAt(refuelContinueTime, refuelEndTime, Convert.ToInt32(level), Convert.ToInt32(litersRequired));

          yield return env.Timeout(secondRefuelDuration);
        } else {
          var refuelDuration = TimeSpan.FromSeconds(litersRequired / RefuelingSpeed);
          yield return fuelPump.Get(litersRequired);

          // Car tank fill visualization
          DateTime refuelStartTime = env.Now;
          DateTime refuelEndTime = env.Now + refuelDuration;
          int entryFrame = Convert.ToInt32((refuelStartTime - env.StartDate).TotalSeconds * env.AnimationBuilder.FPS) + 1;
          int exitFrame = Convert.ToInt32((refuelEndTime - env.StartDate).TotalSeconds * env.AnimationBuilder.FPS);
          int refuelDurationFrames = exitFrame - entryFrame + 1;

          AdvancedRect carTankRect = new AdvancedRect(
            carRect.X,
            250,
            util.GetIntValueAt(refuelStartTime, refuelEndTime, 0, Convert.ToInt32(litersRequired)),
            CarHeight);

          env.AnimationBuilder.Animate(
            name + "Tank",
            carTankRect,
            carTankStyle,
            (Func<int, bool>) (t => gasStation.UsedBy(thisProcess)));

          yield return env.Timeout(refuelDuration);
        }
        FreeGasStation(carRect.X.Value == 275 ? 1 : 2);
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

    private IEnumerable<Event> TankTruck(string name, Simulation env, Container fuelPump) {
      // Tank truck visualization
      Process thisProcess = env.ActiveProcess;
      AdvancedStyle truckStyle = new AdvancedStyle("blue", "blue", 1);
      AdvancedRect truckRect = new AdvancedRect(575, 550, 50, 100);
      env.AnimationBuilder.Animate(
        name,  
        truckRect,
        truckStyle, 
        (Func<int, bool>)(t => fuelPump.PutBy(thisProcess)));

      // Arrives at the gas station after a certain delay and refuels it.
      yield return env.Timeout(TankTruckTime);
      env.Log("Tank truck arriving at time {0}", env.Now);

      var amount = fuelPump.Capacity - fuelPump.Level;
      yield return fuelPump.Put(amount);
      env.Log("Tank truck finished refuelling {0} liters at time {1}.", amount, env.Now);
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
      env.Log("== Gas Station refuelling pull animation ==");

      // BuildAnimation has to be turned on before first Animation is created
      env.AnimationBuilder.DebugAnimation = false;
      env.AnimationBuilder.EnableAnimation = true;
      env.AnimationBuilder.Player = new HtmlPlayer();

      util = env.AnimationBuilder.GetAnimationUtil();

      // Gas station queue visualization
      Rect queueRect = new Rect(10, 50, 50, 50);
      Style queueStyle = new Style("red", "red", 1);
      QueueAnimation queue = env.AnimationBuilder.AnimateQueue("gasStationQueue", queueRect, queueStyle, 100, 20);

      var gasStation = new Resource(env, 2, queue) {
        QueueLength = new TimeSeriesMonitor(env, name: "Waiting cars", collect: true),
        WaitingTime = new SampleMonitor(name: "Waiting time", collect: true),
        Utilization = new TimeSeriesMonitor(env, name: "Station utilization"),
      };

      // Fuel pump level visualization
      Rect fullFuelPumpRect = new Rect(275, 550, 250, GasStationSize);
      Style fuelPumpTankStyle = new Style("black", "black", 1);
      LevelAnimation level = env.AnimationBuilder.AnimateLevel("fuelPumpTank", fullFuelPumpRect, fuelPumpTankStyle);

      var fuelPump = new Container(env, GasStationSize, GasStationSize, level) {
        Fillrate = new TimeSeriesMonitor(env, name: "Tank fill rate")
      };

      // Gas station visualization
      AdvancedRect gasStationLeftRect = new AdvancedRect(275, 350, 50, 100);
      AdvancedRect gasStationRightRect = new AdvancedRect(475, 350, 50, 100);
      AdvancedStyle gasStationStyle = new AdvancedStyle("grey", "grey", 1);
      env.AnimationBuilder.Animate("gasStationLeft", gasStationLeftRect, gasStationStyle, true);
      env.AnimationBuilder.Animate("gasStationRight", gasStationRightRect, gasStationStyle, true);

      // Fuel pump visualization
      AdvancedRect fuelPumpRect = new AdvancedRect(275, 550, 250, GasStationSize);
      AdvancedStyle fuelPumpStyle = new AdvancedStyle("none", "black", 1); 
      env.AnimationBuilder.Animate("fuelPump", fuelPumpRect, fuelPumpStyle, true);

      // Group Test
      AdvancedGroup carGroup = new AdvancedGroup(
        util.GetIntValueAt(env.StartDate + TimeSpan.FromMinutes(20), env.StartDate + TimeSpan.FromMinutes(50), 0, 1000), 
        0, 
        50, 
        35);
      AdvancedRect carTop = new AdvancedRect(10, 0, 20, 15);
      AdvancedRect carBottom = new AdvancedRect(0, 15, 50, 15);
      AdvancedEllipse wheelLeft = new AdvancedEllipse(7, 30, 5, 5);
      AdvancedEllipse wheelRight = new AdvancedEllipse(43, 30, 5, 5);

      AdvancedStyle carStyle = new AdvancedStyle("green", "none", 0);
      AdvancedStyle wheelStyle = new AdvancedStyle("black", "none", 0);
      AdvancedStyle modCarStyle = new AdvancedStyle(
        util.GetIfTimeBetween<string>(env.StartDate + TimeSpan.FromMinutes(60), env.StartDate + TimeSpan.FromMinutes(100), "red", "green"),
        "none",
        0);

      AdvancedGroupAnimation carAnimation = env.AnimationBuilder.Animate("testCar", carGroup, carStyle, true);
      carAnimation.AddChild(
        carTop, 
        modCarStyle, 
        true);
      carAnimation.AddChild(carBottom, carStyle, true);
      carAnimation.AddChild(wheelLeft, wheelStyle, true);
      carAnimation.AddChild(wheelRight, wheelStyle, true);

      // Text Test

      env.Process(GasStationControl(env, fuelPump));
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
