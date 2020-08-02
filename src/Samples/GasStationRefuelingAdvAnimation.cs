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
using SimSharp.Visualization.Processor;

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
    private const int RefuelingSpeed = 1; // liters / second

    //private static readonly TimeSpan TankTruckTime = TimeSpan.FromMinutes(10); // Minutes it takes the tank truck to arrive
    //private static readonly TimeSpan MinTInter = TimeSpan.FromMinutes(5); // Create a car every min seconds
    //private static readonly TimeSpan MaxTInter = TimeSpan.FromMinutes(50); // Create a car every max seconds
    //private static readonly TimeSpan SimTime = TimeSpan.FromMinutes(150); // Simulation time

    private static readonly TimeSpan TankTruckTime = TimeSpan.FromMinutes(1); // Minutes it takes the tank truck to arrive
    private static readonly TimeSpan MinTInter = TimeSpan.FromSeconds(10); // Create a car every min seconds
    private static readonly TimeSpan MaxTInter = TimeSpan.FromSeconds(20); // Create a car every max seconds
    private static readonly TimeSpan SimTime = TimeSpan.FromMinutes(3); // Simulation time

    private static readonly int CarHeight = 70; // Height of car rectangles
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
        AdvancedGroup fullCar = new AdvancedGroup(GetFreeGasStation() == 1 ? 275 : 475, 250, Convert.ToInt32(FuelTankSize)+70, CarHeight);
        AdvancedRect carTop = new AdvancedRect(20, 5, 65, 30);
        AdvancedRect carBottom = new AdvancedRect(0, 30, 120, 30);
        AdvancedRect window = new AdvancedRect(25, 10, 55, 20);
        AdvancedRect windowMiddle = new AdvancedRect(50, 10, 5, 35);
        AdvancedRect lightFront = new AdvancedRect(115, 32, 5, 10);
        AdvancedRect lightBack = new AdvancedRect(0, 32, 5, 10);
        AdvancedEllipse wheelBack = new AdvancedEllipse(25, 55, 15, 15);
        AdvancedEllipse wheelFront = new AdvancedEllipse(95, 55, 15, 15);
        AdvancedEllipse wheelBackMiddle = new AdvancedEllipse(25, 55, 7, 7);
        AdvancedEllipse wheelFronMiddle = new AdvancedEllipse(95, 55, 7, 7);

        AdvancedStyle fullCarStyle = new AdvancedStyle("green", "none", 0);
        AdvancedStyle fuelingCarStyle = new AdvancedStyle("black", "none", 0);
        AdvancedStyle fuelingWindowStyle = new AdvancedStyle("darkgreen", "none", 0);
        AdvancedStyle lightFrontStyle = new AdvancedStyle("gold", "black", 1);
        AdvancedStyle lightBackStyle = new AdvancedStyle("red", "black", 1);
        AdvancedStyle wheelStyle = new AdvancedStyle("dimgrey", "none", 0);
        AdvancedStyle wheelMiddleStyle = new AdvancedStyle("lightgrey", "none", 0);

        if (litersRequired > fuelPump.Level && fuelPump.Level > 0) {
          var level = fuelPump.Level;
          var firstRefuelDuration = TimeSpan.FromSeconds(level / RefuelingSpeed);
          var secondRefuelDuration = TimeSpan.FromSeconds((litersRequired - level) / RefuelingSpeed);
          yield return fuelPump.Get(level); // draw it empty

          // First car tank fill visualization
          DateTime refuelStartTime = env.Now;
          DateTime refuelPauseTime = env.Now + firstRefuelDuration;
          int entryFrame = Convert.ToInt32((refuelStartTime - env.StartDate).TotalSeconds * env.AnimationBuilder.Config.FPS) + 1;
          int pauseFrame = Convert.ToInt32((refuelPauseTime - env.StartDate).TotalSeconds * env.AnimationBuilder.Config.FPS);
          int firstRefuelFrames = pauseFrame - entryFrame + 1;

          AdvancedGroup refuelCar = new AdvancedGroup(
            fullCar.X,
            250,
            util.GetIntValueAt(refuelStartTime, refuelPauseTime, Convert.ToInt32(fuelTankLevel), Convert.ToInt32(fuelTankLevel + level)),
            CarHeight);

          AdvancedGroupAnimation refuelCarAnimation = env.AnimationBuilder.Animate(
            name + "Tank",
            refuelCar,
            fuelingCarStyle,
            (Func<int, bool>) (t => gasStation.UsedBy(thisProcess)));

          refuelCarAnimation.AddChild("carTop", carTop);
          refuelCarAnimation.AddChild("carBottom", carBottom);
          refuelCarAnimation.AddChild("wheelBack", wheelBack);
          refuelCarAnimation.AddChild("wheelFront", wheelFront);

          AdvancedGroupAnimation fullCarAnimation = env.AnimationBuilder.Animate(
            name,
            fullCar,
            fullCarStyle,
            (Func<int, bool>)(t => gasStation.UsedBy(thisProcess)));

          fullCarAnimation.AddChild("carTop", carTop);
          fullCarAnimation.AddChild("carBottom", carBottom);
          fullCarAnimation.AddChild("window", window, fuelingWindowStyle);
          fullCarAnimation.AddChild("windowMiddle", windowMiddle);
          fullCarAnimation.AddChild("lightFront", lightFront, lightFrontStyle);
          fullCarAnimation.AddChild("lightBack", lightBack, lightBackStyle);
          fullCarAnimation.AddChild("wheelBack", wheelBack);
          fullCarAnimation.AddChild("wheelFront", wheelFront);
          fullCarAnimation.AddChild("wheelBackMiddle", wheelBackMiddle, wheelMiddleStyle);
          fullCarAnimation.AddChild("wheelFrontMiddle", wheelFronMiddle, wheelMiddleStyle);

          yield return env.Timeout(firstRefuelDuration);
          yield return fuelPump.Get(litersRequired - level); // wait for the rest

          // Second car tank fill visualization
          DateTime refuelContinueTime = env.Now;
          DateTime refuelEndTime = env.Now + secondRefuelDuration;
          int continueFrame = Convert.ToInt32((refuelContinueTime - env.StartDate).TotalSeconds * env.AnimationBuilder.Config.FPS) + 1;
          int exitFrame = Convert.ToInt32((refuelEndTime - env.StartDate).TotalSeconds * env.AnimationBuilder.Config.FPS);
          int secondRefuelFrames = exitFrame - continueFrame + 1;

          refuelCar.Width = util.GetIntValueAt(refuelContinueTime, refuelEndTime, Convert.ToInt32(fuelTankLevel + level), Convert.ToInt32(FuelTankSize+70));

          yield return env.Timeout(secondRefuelDuration);
        } else {
          var refuelDuration = TimeSpan.FromSeconds(litersRequired / RefuelingSpeed);
          yield return fuelPump.Get(litersRequired);

          // Car tank fill visualization
          DateTime refuelStartTime = env.Now;
          DateTime refuelEndTime = env.Now + refuelDuration;
          int entryFrame = Convert.ToInt32((refuelStartTime - env.StartDate).TotalSeconds * env.AnimationBuilder.Config.FPS) + 1;
          int exitFrame = Convert.ToInt32((refuelEndTime - env.StartDate).TotalSeconds * env.AnimationBuilder.Config.FPS);
          int refuelDurationFrames = exitFrame - entryFrame + 1;

          AdvancedGroup refuelCar = new AdvancedGroup(
            fullCar.X,
            250,
            util.GetIntValueAt(refuelStartTime, refuelEndTime, Convert.ToInt32(fuelTankLevel), Convert.ToInt32(FuelTankSize+70)),
            CarHeight);

          AdvancedGroupAnimation refuelCarAnimation = env.AnimationBuilder.Animate(
            name + "Tank",
            refuelCar,
            fuelingCarStyle,
            (Func<int, bool>) (t => gasStation.UsedBy(thisProcess)));

          refuelCarAnimation.AddChild("carTop", carTop);
          refuelCarAnimation.AddChild("carBottom", carBottom);
          refuelCarAnimation.AddChild("wheelBack", wheelBack);
          refuelCarAnimation.AddChild("wheelFront", wheelFront);

          AdvancedGroupAnimation fullCarAnimation = env.AnimationBuilder.Animate(
            name,
            fullCar,
            fullCarStyle,
            (Func<int, bool>)(t => gasStation.UsedBy(thisProcess)));

          fullCarAnimation.AddChild("carTop", carTop);
          fullCarAnimation.AddChild("carBottom", carBottom);
          fullCarAnimation.AddChild("window", window, fuelingWindowStyle);
          fullCarAnimation.AddChild("windowMiddle", windowMiddle);
          fullCarAnimation.AddChild("lightFront", lightFront, lightFrontStyle);
          fullCarAnimation.AddChild("lightBack", lightBack, lightBackStyle);
          fullCarAnimation.AddChild("wheelBack", wheelBack);
          fullCarAnimation.AddChild("wheelFront", wheelFront);
          fullCarAnimation.AddChild("wheelBackMiddle", wheelBackMiddle, wheelMiddleStyle);
          fullCarAnimation.AddChild("wheelFrontMiddle", wheelFronMiddle, wheelMiddleStyle);

          yield return env.Timeout(refuelDuration);
        }
        FreeGasStation(fullCar.X.Value == 275 ? 1 : 2);
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
      AnimationBuilder animationBuilder = new AnimationBuilder(1000, 1000, "Gas Station Refueling", 1);
      var env = new Simulation(DateTime.Now.Date, rseed, animationBuilder);
      env.Log("== Gas Station refuelling pull animation ==");

      // BuildAnimation has to be turned on before first Animation is created
      animationBuilder.DebugAnimation = false;
      animationBuilder.EnableAnimation = true;
      animationBuilder.Processor = new HtmlPlayer();

      util = animationBuilder.GetAnimationUtil();

      // Gas station queue visualization
      Group carGroup = new Group(0, 20, 120, CarHeight);
      Rect carTop = new Rect(20, 5, 65, 30);
      Rect carBottom = new Rect(0, 30, 120, 30);
      Rect window = new Rect(25, 10, 55, 20);
      Rect windowMiddle = new Rect(50, 10, 5, 35);
      Rect lightFront = new Rect(115, 32, 5, 10);
      Rect lightBack = new Rect(0, 32, 5, 10);
      Ellipse wheelBack = new Ellipse(25, 55, 15, 15);
      Ellipse wheelFront = new Ellipse(95, 55, 15, 15);
      Ellipse wheelBackMiddle = new Ellipse(25, 55, 7, 7);
      Ellipse wheelFronMiddle = new Ellipse(95, 55, 7, 7);

      GroupStyle waitingCarStyle = new GroupStyle("red", "none", 0);
      Style waitingWindowStyle = new Style("darkred", "none", 0);
      Style lightFrontStyle = new Style("gold", "black", 1);
      Style lightBackStyle = new Style("red", "black", 1);
      Style wheelStyle = new Style("dimgrey", "none", 0);
      Style wheelMiddleStyle = new Style("lightgrey", "none", 0);

      waitingCarStyle.AddChild("carTop", carTop);
      waitingCarStyle.AddChild("carBottom", carBottom);
      waitingCarStyle.AddChild("window", window, waitingWindowStyle);
      waitingCarStyle.AddChild("windowMiddle", windowMiddle);
      waitingCarStyle.AddChild("lightFront", lightFront, lightFrontStyle);
      waitingCarStyle.AddChild("lightBack", lightBack, lightBackStyle);
      waitingCarStyle.AddChild("wheelBack", wheelBack, wheelStyle);
      waitingCarStyle.AddChild("wheelFront", wheelFront, wheelStyle);
      waitingCarStyle.AddChild("wheelBackMiddle", wheelBackMiddle, wheelMiddleStyle);
      waitingCarStyle.AddChild("wheelFrontMiddle", wheelFronMiddle, wheelMiddleStyle);

      QueueAnimation queue = animationBuilder.AnimateQueue("gasStationQueue", carGroup, waitingCarStyle, 150, 20);

      var gasStation = new Resource(env, 2, queue) {
        QueueLength = new TimeSeriesMonitor(env, name: "Waiting cars", collect: true),
        WaitingTime = new SampleMonitor(name: "Waiting time", collect: true),
        Utilization = new TimeSeriesMonitor(env, name: "Station utilization"),
      };

      // Fuel pump level visualization
      Rect fullFuelPumpRect = new Rect(275, 550, 270, GasStationSize);
      Style fuelPumpTankStyle = new Style("black", "none", 0);
      LevelAnimation level = animationBuilder.AnimateLevel("fuelPumpTank", fullFuelPumpRect, fuelPumpTankStyle);

      var fuelPump = new Container(env, GasStationSize, GasStationSize, level) {
        Fillrate = new TimeSeriesMonitor(env, name: "Tank fill rate")
      };

      // Gas station visualization
      AdvancedGroup gasStationLeft = new AdvancedGroup(245, 300, 100, 150);
      AdvancedGroup gasStationRight = new AdvancedGroup(445, 300, 100, 150);
      AdvancedRect gasStationRect = new AdvancedRect(30, 50, 70, 100);
      AdvancedRect gasStationWindow = new AdvancedRect(40, 60, 50, 30);
      AdvancedRect hose1 = new AdvancedRect(0, 110, 30, 7);
      AdvancedRect hose2 = new AdvancedRect(0, 0, 7, 110);
      AdvancedRect hose3 = new AdvancedRect(0, 0, 30, 7);
      AdvancedStyle gasStationStyle = new AdvancedStyle("grey", "grey", 1);
      AdvancedStyle gasStationWindowStyle = new AdvancedStyle("white");

      AdvancedGroupAnimation gasStationLeftAnimation = animationBuilder.Animate("gasStationLeft", gasStationLeft, gasStationStyle, true);
      AdvancedGroupAnimation gasStationRightAnimation = animationBuilder.Animate("gasStationRight", gasStationRight, gasStationStyle, true);

      gasStationLeftAnimation.AddChild("gasStationRect", gasStationRect, true);
      gasStationLeftAnimation.AddChild("gasStationWindow", gasStationWindow, gasStationWindowStyle, true);
      gasStationLeftAnimation.AddChild("hose1", hose1, true);
      gasStationLeftAnimation.AddChild("hose2", hose2, true);
      gasStationLeftAnimation.AddChild("hose3", hose3, true);

      gasStationRightAnimation.AddChild("gasStationRect", gasStationRect, true);
      gasStationRightAnimation.AddChild("gasStationWindow", gasStationWindow, gasStationWindowStyle, true);
      gasStationRightAnimation.AddChild("hose1", hose1, true);
      gasStationRightAnimation.AddChild("hose2", hose2, true);
      gasStationRightAnimation.AddChild("hose3", hose3, true);

      // Fuel pump visualization
      AdvancedRect fuelPumpRect = new AdvancedRect(275, 550, 270, GasStationSize);
      AdvancedStyle fuelPumpStyle = new AdvancedStyle("none", "black", 1); 
      animationBuilder.Animate("fuelPump", fuelPumpRect, fuelPumpStyle, true);

      // GasStationVisualiszation
      AdvancedText text = new AdvancedText(560, 750, 24);

      AdvancedTextStyle textStyle = new AdvancedTextStyle(
        "black",
        "none",
        0,
        (Func<int, string>)(t => Math.Round(fuelPump.Level, 2).ToString()));

      animationBuilder.Animate("testText", text, textStyle, true);

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
