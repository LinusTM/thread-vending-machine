using System;
using System.Threading;

namespace Flaske_Automat;
class Program {
    static Queue<Bottle> boxOfBottles = new Queue<Bottle>();
    static Queue<Bottle> beerBox = new Queue<Bottle>();
    static Queue<Bottle> sodaBox = new Queue<Bottle>();
    static object _lock = new object();
    static Random random = new Random();

    static void Main(string[] args) {
        Thread bottleProducer = new Thread(ProduceBottles);
        Thread splitter = new Thread(SplitBottles);
        Thread beerConsumer = new Thread(ConsumeBeer);
        Thread sodaConsumer = new Thread(ConsumeSoda);

        bottleProducer.Start();
        splitter.Start();
        beerConsumer.Start();
        sodaConsumer.Start();

        bottleProducer.Join();
        splitter.Join();
        beerConsumer.Join();
        sodaConsumer.Join();
    }

    public static void ProduceBottles() {
        while(true) {
            Monitor.Enter(boxOfBottles);
            while(boxOfBottles.Count == 10) {
                Monitor.Wait(boxOfBottles);
            }

            if(random.Next(0, 2) == 1) {
                Bottle beer = new Beer();
                boxOfBottles.Enqueue(beer);

                Console.WriteLine($"Producer has produced beer{beer.bottleNumber}");
            } else {
                Bottle soda = new Soda();
                boxOfBottles.Enqueue(soda);

                Console.WriteLine($"Producer has produced soda{soda.bottleNumber}");
            }

            Monitor.PulseAll(boxOfBottles);
            Monitor.Exit(boxOfBottles);


        }
    }

    static Bottle currentBottle = new Default();
    public static void SplitBottles() {
        while(true) {
            Monitor.Enter(boxOfBottles);
            while(boxOfBottles.Count == 0) {
                Monitor.Wait(boxOfBottles);
            }

            Monitor.Enter(beerBox);
            Monitor.Enter(sodaBox);
            currentBottle = boxOfBottles.Dequeue();
            if(currentBottle.type == "Beer") {
                beerBox.Enqueue(currentBottle);
                Console.WriteLine($"Moved beer{currentBottle.bottleNumber} to beerBox");

                Monitor.PulseAll(beerBox);
                Monitor.Exit(beerBox);
            } else {
                sodaBox.Enqueue(currentBottle);
                Console.WriteLine($"Moved soda{currentBottle.bottleNumber} to sodaBox");

                Monitor.PulseAll(sodaBox);
                Monitor.Exit(beerBox);
            }


            Monitor.Exit(boxOfBottles);
            Thread.Sleep(1000);
        }
    }


    // --- End consumers --- //
    static Bottle currentBeer = new Default();
    public static void ConsumeBeer() {
        while(true) {
            Monitor.Enter(beerBox);
            while(beerBox.Count == 0) {
                Monitor.Wait(beerBox);
            }

            currentBeer = beerBox.Dequeue();
            Console.WriteLine($" Jeff just drank beer {currentBeer.type}{currentBeer.bottleNumber}");
            Monitor.Exit(beerBox);
        }
    }

    static Bottle currentSoda = new Default();
    public static void ConsumeSoda() {
        while(true) {
            Monitor.Enter(sodaBox);
            while(sodaBox.Count == 0) {
                Monitor.Wait(sodaBox);
            }

            currentSoda = sodaBox.Dequeue();
            Console.WriteLine($" Greg just drank soda {currentSoda.type}{currentSoda.bottleNumber}");
            Monitor.Exit(sodaBox);
        }
    }
}


// --- Bottles --- //
public class Bottle {
    static int bottleCount;

    internal int bottleNumber;
    internal string type;

    public Bottle(string type) {
        bottleCount++;
        this.bottleNumber = bottleCount;
        this.type = type;
    }
}

public class Default : Bottle {
    public Default() : base("") { }
}

public class Beer : Bottle {
    public Beer() : base("Beer") { }
}

public class Soda : Bottle {
    public Soda() : base("Soda") { }
}

