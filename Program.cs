using System;
using System.Threading;

namespace Flaske_Automat;
class Program {
    // Shared resources
    static Queue<Bottle> boxOfBottles = new Queue<Bottle>();
    static Queue<Bottle> beerBox = new Queue<Bottle>();
    static Queue<Bottle> sodaBox = new Queue<Bottle>();
    static Random random = new Random();

    static void Main(string[] args) {
        // Creating, starting and stopping threads
        Thread produceBottles = new Thread(ProduceBottles);
        Thread sortBottles = new Thread(SortBottles);
        Thread consumeBeer = new Thread(ConsumeBeer);
        Thread consumeSoda = new Thread(ConsumeSoda);

        produceBottles.Start();
        sortBottles.Start();
        consumeBeer.Start();
        consumeSoda.Start();

        produceBottles.Join();
        sortBottles.Join();
        consumeBeer.Join();
        consumeSoda.Join();
    }

    public static void ProduceBottles() {
        while(true) {
            Monitor.Enter(boxOfBottles);
            try {
                // Check/wait if boxOfBottles is full
                while(boxOfBottles.Count == 10) {
                    Thread.Sleep(100/15);
                    Monitor.Wait(boxOfBottles);
                }

                // Initiate new beer or soda depending on random number
                switch(random.Next(0, 2)) {
                    case 0:
                        Bottle beer = new Beer();
                        boxOfBottles.Enqueue(beer);

                        Console.WriteLine($"PRODUCER: Has produced {beer.type}{beer.number}");
                        break;
                    default:
                        Bottle soda = new Soda();
                        boxOfBottles.Enqueue(soda);

                        Console.WriteLine($"PRODUCER: Has produced {soda.type}{soda.number}");
                        break;
                }

                // Notify threads: Update to boxOfBottles queue
                Monitor.PulseAll(boxOfBottles);
            }
            finally {
                Monitor.Exit(boxOfBottles);
            }

            Thread.Sleep(500);
        }
    }

    static Bottle currentBottle = new Default();
    public static void SortBottles() {
        while(true) {
            Monitor.Enter(boxOfBottles);

            try {
                // Check/wait if no Bottles are in boxOfBottles
                while(boxOfBottles.Count == 0) {
                    Thread.Sleep(100/15);
                    Monitor.Wait(boxOfBottles);
                }

                currentBottle = boxOfBottles.Dequeue();
                switch(currentBottle.type) {
                    case "Beer":
                        Monitor.Enter(beerBox);

                        try {
                            // Put in beerBox if Bottle type == "Beer"
                            beerBox.Enqueue(currentBottle);
                            Console.WriteLine($"SORTER: Moved beer{currentBottle.number} to beerBox");

                            // Notify threads: Update to beerBox queue
                            Monitor.PulseAll(beerBox);
                        } 
                        finally {
                            Monitor.Exit(beerBox);
                        }
                        break;
                    case "Soda":
                        Monitor.Enter(sodaBox);
                        try {
                            // Put in sodaBox if Bottle type == "Soda"
                            sodaBox.Enqueue(currentBottle);
                            Console.WriteLine($"SORTER: Moved soda{currentBottle.number} to sodaBox");

                            // Notify threads: Update to sodaBox queue
                            Monitor.PulseAll(sodaBox);
                        }
                        finally {
                            Monitor.Exit(sodaBox);
                        }
                        break;
                }
            } 
            finally {
                Monitor.Exit(boxOfBottles);
            }

            Thread.Sleep(500);
        }
    }


    // --- End consumers --- //
    static Bottle currentBeer = new Default();
    public static void ConsumeBeer() {
        while(true) {
            Monitor.Enter(beerBox);

            try {
                // Check/wait if no available beer
                while(beerBox.Count == 0) {
                    Thread.Sleep(100/15);
                    Monitor.Wait(beerBox);
                }

                // Toss nearest beer and notify
                currentBeer = beerBox.Dequeue();
                Console.WriteLine($"JEFF: just drank {currentBeer.type}{currentBeer.number}");
            }
            finally {
                Monitor.Exit(beerBox);
            }
        }
    }

    static Bottle currentSoda = new Default();
    public static void ConsumeSoda() {
        while(true) {
            Monitor.Enter(sodaBox);

            try {
                // Check/wait if no available soda
                while(sodaBox.Count == 0) {
                    Thread.Sleep(100/15);
                    Monitor.Wait(sodaBox);
                }

                // Toss nearest soda and notify
                currentSoda = sodaBox.Dequeue();
                Console.WriteLine($"Greg:  just drank {currentSoda.type}{currentSoda.number}");
            }
            finally {
                Monitor.Exit(sodaBox);
            }
        }
    }
}


// --- Bottles --- //
public abstract class Bottle {
    static int bottleCount;

    internal int number;
    internal string type;

    public Bottle(string type) {
        bottleCount++;
        this.number = bottleCount;
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

