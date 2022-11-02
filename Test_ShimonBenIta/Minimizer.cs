using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Timers;

namespace Test_ShimonBenIta
{
    public class Minimizer
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        static extern int SetWindowText(IntPtr hWnd, string text);

        // Timers and Windows Lists
        static List<KeyValuePair<int, System.Timers.Timer>> timersList = new List<KeyValuePair<int, System.Timers.Timer>>();
        static List<KeyValuePair<int, Process>>? windowsList = null;

        const string miniString = " Minimize Timer:";

        public static void consoleApp()
        {
            int choice = 0;
            
            do
            {
                try
                {
                    printMenu();
                    choice = Convert.ToInt32(Console.ReadLine());

                    if(choice < 1 || choice > 4)
                        throw new Exception();

                    refreshWindowsList(out windowsList);

                    Console.Clear();
                    switch (choice)
                    {
                    case 1: // Prints the windows list
                        windowsList.ForEach(window =>
                        {
                            Console.WriteLine($"Program ID {window.Key} | Process ID: {window.Value.Id} | Window title: {window.Value.MainWindowTitle}");
                        });
                        break;
                    case 2: // Set Timer to minimize
                        Console.WriteLine("Please insert window ID to minimize");
                        int winId = Convert.ToInt32(Console.ReadLine());

                        
                        KeyValuePair<int, Process> winToClose = new KeyValuePair<int, Process>();
                        bool found = false;
                        windowsList.ForEach(window =>
                        {
                            if (winId == window.Key)
                            {
                                found = true;
                                winToClose = window;
                            }
                        });

                        if (!found)
                            throw new Exception();

                        Console.WriteLine("Please insert time to minimize (in seconds)");
                        int timeInSeconds = (int)Convert.ToUInt32(Console.ReadLine());

                        System.Timers.Timer aTimer = new System.Timers.Timer();
                        var thisDate = DateTime.Now;
                        aTimer.Elapsed += (sender, args) => updateTitleTimer(winToClose, timeInSeconds, thisDate); 
                        aTimer.Interval = 1000;
                        aTimer.Enabled = true;

                        // Add to timers list
                        timersList.Add(new KeyValuePair<int, System.Timers.Timer>(winId, aTimer));
                        Console.Clear();
                        break;

                    case 3: // Abort Timer to minimize
                        Console.WriteLine("Please insert window ID to minimize");
                        winId = Convert.ToInt32(Console.ReadLine());

                        windowsList.ForEach(window =>
                        {
                            if (winId == window.Key)
                            {
                                abortTimer(winId);
                            }
                        });
                        Console.Clear();

                        break;
                    case 4: // Exit code OK
                        Environment.Exit(0);
                        return;
                    }
                }
                catch (Exception)
                {
                    Console.Clear();
                    Console.WriteLine("Input Error, Please try again");
                }
            } while (true);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <param name="timeInSeconds"></param>
        /// <param name="StartTime"></param>
        static void updateTitleTimer(KeyValuePair<int, Process> window, int timeInSeconds, DateTime StartTime)
        {
            TimeSpan t = DateTime.Now - StartTime;
            if(t.TotalSeconds >= timeInSeconds)
            {
                ShowWindow(window.Value.MainWindowHandle, 6);
                abortTimer(window.Key);
            }
            else
                SetWindowText(window.Value.MainWindowHandle, $"{window.Value.MainWindowTitle.Split(miniString)[0]}{miniString} {t.TotalSeconds.ToString("0")}");
        }

        /// <summary>
        /// Made an ABORT to a window by win ID
        /// </summary>
        /// <param name="winId"></param>
        static void abortTimer(int winId)
        {
            var itemToRemove = timersList.Single(r => r.Key == winId);
            var windows = windowsList.Single(r => r.Key == winId);
            SetWindowText(windows.Value.MainWindowHandle, $"{windows.Value.MainWindowTitle.Split(miniString)[0]}");
            itemToRemove.Value.Stop();
            timersList.Remove(itemToRemove);
        }

        /// <summary>
        /// Prints the menu
        /// </summary>
        static void printMenu()
        {
            Console.WriteLine("--------------------------------------------------------------------");
            Console.WriteLine("- Select one of the following options:");
            Console.WriteLine("--------------------------------------------------------------------");
            Console.WriteLine("- 1. Show a list of windows details");
            Console.WriteLine("- 2. Set a window to be minimized");
            Console.WriteLine("- 3. Abort the minimize of a window");
            Console.WriteLine("- 4. Exit");
            Console.WriteLine("--------------------------------------------------------------------");
        }
        
        /// <summary>
        /// Refresh the windows list
        /// </summary>
        /// <param name="windowsList"></param>
        static void refreshWindowsList(out List<KeyValuePair<int, Process>> windowsList)
        {
            var processlist = Process.GetProcesses();
            windowsList = new List<KeyValuePair<int, Process>>();
            int pId = 0;
            foreach (Process process in processlist)
            {
                if(!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    windowsList.Add(new KeyValuePair<int, Process>(pId, process));
                    pId++;
                }
            }
        }
    }
}
