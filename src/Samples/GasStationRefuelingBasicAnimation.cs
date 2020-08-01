#region License Information
/*
 * This file is part of SimSharp which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using SimSharp.Visualization.Basic.Shapes;
using SimSharp.Visualization.Basic;
using SimSharp.Visualization;
using SimSharp.Visualization.Basic.Resources;
using SimSharp.Visualization.Processor;
using SimSharp.Visualization.Basic.Styles;

namespace SimSharp.Samples {
  public class GasStationRefuelingBasicAnimation {
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

    // Used for Visualization
    private static readonly int CarHeight = 70; // Height of car rectangles
    private static bool[] gasStations = { true, true };

    // Car Visualisation
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

    Style waitingWindowStyle = new Style("darkred", "none", 0);
    Style lightFrontStyle = new Style("gold", "black", 1);
    Style lightBackStyle = new Style("red", "black", 1);
    Style wheelStyle = new Style("dimgrey", "none", 0);
    Style wheelMiddleStyle = new Style("lightgrey", "none", 0);

    private int GetFreeGasStation() {
      if (gasStations[0]) {
        gasStations[0] = false;
        return 1;
      }
      else {
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
        Group fullCar = new Group(GetFreeGasStation() == 1 ? 275 : 475, 250, Convert.ToInt32(FuelTankSize)+70, CarHeight);
        Group emptyCar = new Group(fullCar.X, fullCar.Y, Convert.ToInt32(fuelTankLevel), CarHeight);
        GroupStyle fullCarStyle = new GroupStyle("green", "none", 0);
        GroupStyle fuelingCarStyle = new GroupStyle("black", "none", 0);
        Style fuelingWindowStyle = new Style("darkgreen", "none", 0);

        fullCarStyle.AddChild("carTop", carTop);
        fullCarStyle.AddChild("carBottom", carBottom);
        fullCarStyle.AddChild("window", window, fuelingWindowStyle);
        fullCarStyle.AddChild("windowMiddle", windowMiddle);
        fullCarStyle.AddChild("lightFront", lightFront, lightFrontStyle);
        fullCarStyle.AddChild("lightBack", lightBack, lightBackStyle);
        fullCarStyle.AddChild("wheelBack", wheelBack, wheelStyle);
        fullCarStyle.AddChild("wheelFront", wheelFront, wheelStyle);
        fullCarStyle.AddChild("wheelBackMiddle", wheelBackMiddle, wheelMiddleStyle);
        fullCarStyle.AddChild("wheelFrontMiddle", wheelFronMiddle, wheelMiddleStyle);

        fuelingCarStyle.AddChild("carTop", carTop);
        fuelingCarStyle.AddChild("carBottom", carBottom);
        fuelingCarStyle.AddChild("wheelBack", wheelBack);
        fuelingCarStyle.AddChild("wheelFront", wheelFront);

        if (litersRequired > fuelPump.Level && fuelPump.Level > 0) {
          var level = fuelPump.Level;
          var firstRefuelDuration = TimeSpan.FromSeconds(level / RefuelingSpeed);
          var secondRefuelDuration = TimeSpan.FromSeconds((litersRequired - level) / RefuelingSpeed);
          yield return fuelPump.Get(level); // draw it empty

          // First car tank fill visualization
          Group tempCar = new Group(emptyCar.X, emptyCar.Y, Convert.ToInt32(fuelTankLevel + level), CarHeight);
          Animation fillCarAnimation = env.AnimationBuilder.Animate(name+"Tank", emptyCar, tempCar, env.Now, env.Now + firstRefuelDuration, fuelingCarStyle);
          Animation carAnimation = env.AnimationBuilder.Animate(name, fullCar, fullCarStyle);

          yield return env.Timeout(firstRefuelDuration);
          yield return fuelPump.Get(litersRequired - level); // wait for the rest

          // Second car tank fill visualization
          fillCarAnimation.Update(fullCar, env.Now, env.Now + secondRefuelDuration, false);
          carAnimation.Update(fullCar, env.Now, env.Now + secondRefuelDuration, false); 

          yield return env.Timeout(secondRefuelDuration);
        } else {
          var refuelDuration = TimeSpan.FromSeconds(litersRequired / RefuelingSpeed);
          yield return fuelPump.Get(litersRequired);

          // Car tank fill visualization
          env.AnimationBuilder.Animate(name+"Tank", emptyCar, fullCar, env.Now, env.Now + refuelDuration, fuelingCarStyle, false);
          env.AnimationBuilder.Animate(name, fullCar, env.Now, env.Now + refuelDuration, fullCarStyle, false);

          yield return env.Timeout(refuelDuration);
        }
        FreeGasStation(fullCar.X == 275 ? 1 : 2);            
        env.Log("{0} finished refueling in {1} seconds.", name, (env.Now - start).TotalSeconds);
      }
    }

    private IEnumerable<Event> GasStationVisualization(Simulation env, Container fuelPump) {
      /*
       * Update the visualization of the fuel pump if the level changes 
       */
      Text fuelPumpText = new Text(560, 750, fontSize: 24);
      TextStyle fuelPumpTextStyle = new TextStyle("black", "none", 0, GasStationSize.ToString());
      Animation textAnimation = env.AnimationBuilder.Animate("fuelPumpText", fuelPumpText, fuelPumpTextStyle);

      while (true) {
        yield return fuelPump.WhenChange();
        // Visualization has to be updated
        TextStyle newfuelPumpTextStyle = new TextStyle("black", "none", 0, Math.Round(fuelPump.Level, 2).ToString());
        textAnimation.Update(fuelPumpText, newfuelPumpTextStyle);
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
      env.Log("== Gas Station refuelling push animation ==");

      // BuildAnimation has to be turned on before first Animation is created
      animationBuilder.DebugAnimation = false;
      animationBuilder.EnableAnimation = true;
      animationBuilder.Processor = new HtmlPlayer();

      // Gas station queue visualization
      Group carGroup = new Group(0, 20, 120, CarHeight);
      GroupStyle waitingCarStyle = new GroupStyle("red", "none", 0);

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

      // Gas station visualization
      Group gasStationLeft = new Group(245, 300, 100, 150);
      Group gasStationRight = new Group(445, 300, 100, 150);
      Rect gasStationRect = new Rect(30, 50, 70, 100);
      Rect gasStationWindow = new Rect(40, 60, 50, 30);
      Rect hose1 = new Rect(0, 110, 30, 7);
      Rect hose2 = new Rect(0, 0, 7, 110);
      Rect hose3 = new Rect(0, 0, 30, 7);
      GroupStyle gasStationStyle = new GroupStyle("grey", "grey", 1);
      Style gasStationWindowStyle = new Style("white", "none", 0);

      gasStationStyle.AddChild("gasStationRect", gasStationRect);
      gasStationStyle.AddChild("gasStationWindow", gasStationWindow, gasStationWindowStyle);
      gasStationStyle.AddChild("hose1", hose1);
      gasStationStyle.AddChild("hose2", hose2);
      gasStationStyle.AddChild("hose3", hose3);

      animationBuilder.Animate("gasStationLeft", gasStationLeft, env.StartDate, gasStationStyle);
      animationBuilder.Animate("gasStationRight", gasStationRight, env.StartDate, gasStationStyle);

      // Fuel pump level visualization
      Rect fullFuelPumpRect = new Rect(275, 550, 270, GasStationSize);
      Style fuelPumpTankStyle = new Style("black", "black", 1);
      LevelAnimation level = animationBuilder.AnimateLevel("fuelPumpTank", fullFuelPumpRect, fuelPumpTankStyle);

      var fuelPump = new Container(env, GasStationSize, GasStationSize, level) {
        Fillrate = new TimeSeriesMonitor(env, name: "Tank fill rate")
      };

      // Fuel pump visualization
      Rect fuelPumpRect = new Rect(275, 550, 270, GasStationSize);
      Style fuelPumpStyle = new Style("none", "black", 3);
      animationBuilder.Animate("fuelPump", fuelPumpRect, env.StartDate, fuelPumpStyle);

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
