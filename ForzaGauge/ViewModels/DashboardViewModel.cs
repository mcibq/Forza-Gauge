using ForzaGauge.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ForzaGauge.UDP;
using System.Windows.Threading;
using System.Configuration;
using System.Windows;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections;
using System.Diagnostics.Contracts;

namespace ForzaGauge.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        public ICommand ConnectCommand { get; }
        public ICommand RpmPlusCommand { get; }
        public ICommand RpmMinusCommand { get; }
        public ICommand ResetCommand { get; }
        //public ICommand ProgressBarCommand { get; set; }
        public ICommand FPSCommand { get; }
        public List<float> DistanceList { get; set; }
        private static DataPacket data = new DataPacket();
        private bool raceMode = false;
        private bool isPaused = false;
        private bool receivingDataFlag = false;
        private bool raceStart = false;
        private bool raceRestart = false;
        private bool timeReverse = false;
        private float tempRaceTime = 0;

        private readonly DispatcherTimer _timer;
        private readonly DispatcherTimer _timerRace;
        private int rpmRedValue = 84;
        private int fpsMS = 32;
        private const int canvasWidth = 300;
        private float lastPosX = 0;
        private float lastPosY = 0;
        private float lastPosZ = 0;
        private bool _udpConnectionStart = false;
        public bool UDPConnectionStart
        {
            get { return _udpConnectionStart; }
            set { _udpConnectionStart = value; OnPropertyChanged(nameof(UDPConnectionStart)); }
        }
        private string _consoleText;
        public string ConsoleText
        {
            get { return _consoleText; }
            set { _consoleText = value; OnPropertyChanged(nameof(ConsoleText)); }
        }

        private int _distanceMeters = 0;
        public int DistanceMeters
        {
            get { return _distanceMeters; }
            set { _distanceMeters = value; OnPropertyChanged(nameof(DistanceMeters)); }
        }

        private PointCollection _gForcePoints;
        public PointCollection GForcePoints
        {
            get { return _gForcePoints; }
            set { _gForcePoints = value; OnPropertyChanged(nameof(GForcePoints)); }
        }


        private string _windowFPS;
        public string WindowFPS
        {
            get { return _windowFPS; }
            set { _windowFPS = value; OnPropertyChanged(nameof(WindowFPS)); }
        }

        private string _forzaGear;
        public string ForzaGear
        {
            get { return _forzaGear; }
            set { _forzaGear = value; OnPropertyChanged(nameof(ForzaGear)); }
        }

        private int _tireFLTemp;
        public int TireFLTemp
        {
            get { return _tireFLTemp; }
            set { _tireFLTemp = value; OnPropertyChanged(nameof(TireFLTemp)); }
        }

        private int _tireFRTemp;
        public int TireFRTemp
        {
            get { return _tireFRTemp; }
            set { _tireFRTemp = value; OnPropertyChanged(nameof(TireFRTemp)); }
        }

        private int _tireRLTemp;
        public int TireRLTemp
        {
            get { return _tireRLTemp; }
            set { _tireRLTemp = value; OnPropertyChanged(nameof(TireRLTemp)); }
        }

        private int _tireRRTemp;
        public int TireRRTemp
        {
            get { return _tireRRTemp; }
            set { _tireRRTemp = value; OnPropertyChanged(nameof(TireRRTemp)); }
        }


        private Brush _progressBarColor;
        public Brush ProgressBarColor
        {
            get { return _progressBarColor; }
            set { _progressBarColor = value; OnPropertyChanged(nameof(ProgressBarColor)); }
        }

        private Brush _tireFLColor;
        public Brush TireFLColor
        {
            get { return _tireFLColor; }
            set { _tireFLColor = value; OnPropertyChanged(nameof(TireFLColor)); }
        }
        private Brush _tireFRColor;
        public Brush TireFRColor
        {
            get { return _tireFRColor; }
            set { _tireFRColor = value; OnPropertyChanged(nameof(TireFRColor)); }
        }
        private Brush _tireRLColor;
        public Brush TireRLColor
        {
            get { return _tireRLColor; }
            set { _tireRLColor = value; OnPropertyChanged(nameof(TireRLColor)); }
        }
        private Brush _tireRRColor;
        public Brush TireRRColor
        {
            get { return _tireRRColor; }
            set { _tireRRColor = value; OnPropertyChanged(nameof(TireRRColor)); }
        }

        private string _forzaStatus;
        public string ForzaStatus
        {
            get { return _forzaStatus; }
            set { _forzaStatus = value; OnPropertyChanged(nameof(ForzaStatus)); }
        }
        private float _forzaRaceTime;
        public float ForzaRaceTime
        {
            get { return _forzaRaceTime; }
            set { _forzaRaceTime = value; OnPropertyChanged(nameof(ForzaRaceTime)); }
        }
        private int _forzaSpeed;
        public int ForzaSpeed
        {
            get { return _forzaSpeed; }
            set { _forzaSpeed = value; OnPropertyChanged(nameof(ForzaSpeed)); }
        }
        private int _forzaSpeedMax;
        public int ForzaSpeedMax
        {
            get { return _forzaSpeedMax; }
            set { _forzaSpeedMax = value; OnPropertyChanged(nameof(ForzaSpeedMax)); }
        }
        private int _forzaSpeedAvg;
        public int ForzaSpeedAvg
        {
            get { return _forzaSpeedAvg; }
            set { _forzaSpeedAvg = value; OnPropertyChanged(nameof(ForzaSpeedAvg)); }
        }
        private float _forzaRpm;
        public float ForzaRpm
        {
            get { return _forzaRpm; }
            set { _forzaRpm = value; OnPropertyChanged(nameof(ForzaRpm)); RpmValueChanged(value); }
        }
        private int _forzaRpmX1000;
        public int ForzaRpmX1000
        {
            get { return _forzaRpmX1000; }
            set { _forzaRpmX1000 = value; OnPropertyChanged(nameof(ForzaRpmX1000)); }

        }

        private float _forzaCurrentLapTime;
        public float ForzaCurrentLapTime
        {
            get { return _forzaCurrentLapTime; }
            set { _forzaCurrentLapTime = value; OnPropertyChanged(nameof(ForzaCurrentLapTime)); }
        }

        private const int FORZA_DATA_OUT_PORT = 5300;
        public DashboardViewModel()
        {
            ConnectCommand = new ConnectToUDPCommand(this);
            RpmPlusCommand = new DashboardRpmPlusCommand(this);
            RpmMinusCommand = new DashboardRpmMinusCommand(this);
            TireFLColor = new SolidColorBrush(Colors.Transparent);
            TireFRColor = new SolidColorBrush(Colors.Transparent);
            TireRLColor = new SolidColorBrush(Colors.Transparent);
            TireRRColor = new SolidColorBrush(Colors.Transparent);
            ProgressBarColor = new SolidColorBrush(Color.FromRgb(0, 146, 255));
            ForzaGear = "N";
            //ProgressBarCommand = new ProgressBarRpmValueChangedCommand(this);
            WindowFPS = "30 FPS";
            ResetCommand = new ResetDashboardCommand(this);
            FPSCommand = new ChangeFPSCommand(this);
            //ForzaRpmX1000 = 0;
            ForzaStatus = "Not connected";
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(fpsMS) };
            _timer.Start();
            _timer.Tick += _timerCalculations_Tick;
            DistanceList = new List<float>();
            _timerRace = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _timerRace.Start();
            _timerRace.Tick += _timerRaceCalculations_Tick;


        }

        private void _timerRaceCalculations_Tick(object sender, EventArgs e)
        {
            if (raceMode)
            {
                if (raceStart == true)
                {
                    lastPosX = data.PositionX;
                    lastPosY = data.PositionY;
                    lastPosZ = data.PositionZ;

                    raceStart = false;
                    return;

                }
                
                if (isPaused)
                {
                    if (ForzaRaceTime > 0)
                    {
                        // przy pauzie w wyscigu tylko wywietl avg speed
                        ForzaSpeedAvg = Convert.ToInt32(3.6 * DistanceMeters / ForzaRaceTime);
                    }

                    //tempRaceTime = ForzaRaceTime;
                }
                else
                {
                    // unpaused
                    int timeDifferenceMilliseconds = 0;
                    if (tempRaceTime > ForzaRaceTime)
                    {
                        timeReverse = true;
                        timeDifferenceMilliseconds = Convert.ToInt32(1000 * (tempRaceTime - ForzaRaceTime));
                        tempRaceTime = 0;
                    }
                    else
                    {
                        timeReverse = false;
                        //timeDifferenceMilliseconds = 0;
                        tempRaceTime = 0;
                        
                    }
                    float x1 = data.PositionX;
                    float y1 = data.PositionY;
                    float z1 = data.PositionZ;
                    float x2 = lastPosX;
                    float y2 = lastPosY;
                    float z2 = lastPosZ;
                    double vectorLength = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2) + Math.Pow(z2 - z1, 2));
                    float distanceTemp = (float)Math.Round(vectorLength, 1);
                    //if (tempCurrentLapTime > lastLapTime)
                    if (timeReverse == true)
                    {

                        for (int i = 0; i < (timeDifferenceMilliseconds / 100); i++)
                        {
                            if (DistanceList.Count > 0)
                                DistanceList.RemoveAt(DistanceList.Count - 1);
                        }
                        if (DistanceList.Count == 0)
                        {
                            // poczatek wyscigu jezeli cofniemy mocno na poczatek do samego 0
                            raceStart = true;
                        }

                    }
                    else
                    {
                        DistanceList.Add(distanceTemp);

                    }

                    DistanceMeters = Convert.ToInt32(DistanceList.Sum());

                    if (ForzaRaceTime > 0)
                    {
                        ForzaSpeedAvg = Convert.ToInt32(3.6 * DistanceMeters / ForzaRaceTime);
                    }

                    lastPosX = data.PositionX;
                    lastPosY = data.PositionY;
                    lastPosZ = data.PositionZ;
                }
                tempRaceTime = ForzaRaceTime;
            }
            else
            {


                if (DistanceList.Count > 0)
                {
                    DistanceList.Clear();
                }

                raceStart = true;


            }
            ConsoleText = "ForzaRaceTime " + ForzaRaceTime + Environment.NewLine;
            ConsoleText += "ForzaCurrentLapTime " + ForzaCurrentLapTime + Environment.NewLine;
            ConsoleText += "RaceStart " + raceStart + Environment.NewLine;
            ConsoleText += "RaceMode " + raceMode + Environment.NewLine;
            ConsoleText += "TimeReverse " + timeReverse + Environment.NewLine;
            ConsoleText += "isPaused " + isPaused + Environment.NewLine;
            //ConsoleText += "fltire temp " + fltire + Environment.NewLine;
        }

        private void _timerCalculations_Tick(object sender, EventArgs e)
        {
            if (receivingDataFlag)
            {
                //draw  g FORCE
                #region GFORCE
                float forzaGForceX = (float)Math.Round(data.AccelerationX / 9.81, 2);
                float forzaGForceZ = (float)Math.Round(data.AccelerationZ / 9.81, 2);
                int gForceX = Convert.ToInt32(-forzaGForceX * canvasWidth / 10);
                int gForceY = Convert.ToInt32(-forzaGForceZ * canvasWidth / 10);

                int angle = CalcAngle(gForceX, gForceY);
                double angleRad = CalcRad(angle);
                int r = (int)(canvasWidth / 2);
                int x1 = r;
                int y1 = r;
                int x2 = gForceX;
                int y2 = gForceY;
                x2 = (int)(r + x2);
                y2 = (int)(r - y2);
                if (IsVectorLengthExceedingBounds(x1, x2, y1, y2, r))
                {
                    x2 = (int)(r + Math.Cos(angleRad) * r);
                    y2 = (int)(r - Math.Sin(angleRad) * r);
                }
                PointCollection pc = new PointCollection
                {
                    new Point(x1, y1),
                    new Point(x2, y2)
                };
                this.GForcePoints = pc;
                #endregion

                TireFLTemp = (int)Math.Round((((data.TireTempFl - 32) * 5) / 9));
                TireFLColor = TireColor(TireFLTemp);
                TireFRTemp = (int)Math.Round((((data.TireTempFr - 32) * 5) / 9));
                //byte frtire = (byte)Math.Round((((data.TireTempFr - 32) * 5) / 9) * 255 / 350);
                TireFRColor = TireColor(TireFRTemp);
                //TireFRColor = new SolidColorBrush(Color.FromRgb(frtire, 0, 0));
                TireRLTemp = (int)Math.Round((((data.TireTempRl - 32) * 5) / 9));
                TireRLColor = TireColor(TireRLTemp);
                //byte rltire = (byte)Math.Round((((data.TireTempRl - 32) * 5) / 9) * 255 / 350);
                //TireRLColor = new SolidColorBrush(Color.FromRgb(rltire, 0, 0));
                TireRRTemp = (int)Math.Round((((data.TireTempRr - 32) * 5) / 9));
                TireRRColor = TireColor(TireRRTemp);
                //byte rrtire = (byte)Math.Round((((data.TireTempRr - 32) * 5) / 9) * 255 / 350);
                //TireRRColor = new SolidColorBrush(Color.FromRgb(rrtire, 0, 0));

                ForzaGear = data.Gear.ToString();
                ForzaSpeed = (int)(data.Speed * 3.6);
                ForzaRpmX1000 = (int)(data.CurrentEngineRpm / 1000);
                ForzaRaceTime = data.CurrentRaceTime;
                ForzaCurrentLapTime = data.CurrentLapTime;
                if (data.CurrentEngineRpm > 0)
                {
                    ForzaRpm = (float)Math.Round(100 * data.CurrentEngineRpm / data.EngineMaxRpm, 1);
                }
                if (ForzaSpeed > ForzaSpeedMax)
                    ForzaSpeedMax = ForzaSpeed;
                if (ForzaGear == "0")
                    ForzaGear = "R";
                if (ForzaGear == "11")
                    ForzaGear = "N";
            }

        }

        public SolidColorBrush TireColor(int temp)
        {
            int coldLower = 65;
            int coldUpper = 90;
            int warmLower = 90;
            int warmUpper = 130;
            int hotLower = 130;
            int hotUpper = 350;

            if (temp > coldLower && temp < coldUpper)
            {
                //cold
                //color 2
                SolidColorBrush a = new SolidColorBrush(Color.FromRgb(58, 105, 189));
                return a;
            }
            else if (temp >= warmLower && temp < warmUpper)
            {
                //int t1 = temp - hotLower;
                //byte x = (byte)(255 - (hotUpper - hotLower) + t1);
                // d3.interpolateLab(color3, color4)(x)
                SolidColorBrush a = new SolidColorBrush(Color.FromRgb(255, 215, 0));
                return a;
            }
            else if (temp >= hotLower && temp <= hotUpper)
            {
                //int t1 = temp - warmUpper;
                // 
                //byte g = (byte)(15 + (hotUpper - warmUpper) - t1);
                //byte x = (byte)(255 - (hotUpper - hotLower + warmUpper - warmLower) + t1);
                // d3.interpolateLab(color2, color3)(x)
                SolidColorBrush a = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                return a;
            }
            else
            {
                //super cold
                SolidColorBrush a = new SolidColorBrush(Color.FromRgb(58, 189, 255));
                return a;
            }
        }

        public void Reset()
        {
            ForzaSpeedAvg = 0;
            ForzaSpeedMax = 0;
            DistanceMeters = 0;
            DistanceList.Clear();

        }
        public void ChangeFPS()
        {
            // 60 FPS
            if (fpsMS == 32)
            {
                fpsMS = 16;
                WindowFPS = "60 FPS";
            }
            else
            {
                fpsMS = 32;
                WindowFPS = "30 FPS";
            }
            _timer.Interval = TimeSpan.FromMilliseconds(fpsMS);
        }
        public void RpmValueChanged(float rpm)
        {
            if (rpm > rpmRedValue)
            {
                ProgressBarColor = Brushes.Red;
            }
            else
            {
                ProgressBarColor = new SolidColorBrush(Color.FromRgb(0, 146, 255));
            }
        }
        public void RpmPlus()
        {
            if (rpmRedValue < 99)
            {
                rpmRedValue++;
                ForzaRpm = rpmRedValue;
                //RpmValueChanged();
            }
        }
        public void RpmMinus()
        {
            if (rpmRedValue > 50)
            {
                rpmRedValue--;
                ForzaRpm = rpmRedValue;
            }
        }

        public void Connect()

        {
            ForzaStatus = "Connecting";
            _ = UDPScanner();
            UDPConnectionStart = true;
            //await Task.Delay(50);
        }

        public void ReceivingData()
        {

            //ForzaRaceTime = data.CurrentRaceTime;
            //_ = ComputeTasksWithDelay();
        }

        /*public async Task ComputeTasksWithDelay()
        {
            ForzaSpeed = Convert.ToInt32(data.Speed * 3.6);
            ForzaRaceTime = data.CurrentRaceTime.ToString();
            //await Task.Delay(100);
        }*/
        private int CalcAngle(int x, int y)
        {
            int angle = (int)(Math.Atan2(y, x) * 180 / Math.PI);
            if (angle < 0)
                angle = 360 + angle;
            return angle;
        }
        private double CalcRad(int angle)
        {
            return angle * Math.PI / 180;
        }
        private bool IsVectorLengthExceedingBounds(int x1, int x2, int y1, int y2, int radius)
        {
            int vectorLength = Convert.ToInt32(Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)));
            if (vectorLength > radius)
                return true;
            else return false;
        }

        // UDP
        public async Task UDPScanner()
        {
            try
            {
                bool done = false;
                UdpClient client = new UdpClient(FORZA_DATA_OUT_PORT);
                IPEndPoint groupEP = new IPEndPoint(IPAddress.Loopback, FORZA_DATA_OUT_PORT);

                while (!done)
                {

                    await client.ReceiveAsync().ContinueWith(receive =>
                    {
                        var resultBuffer = receive.Result.Buffer;
                        if (!AdjustToBufferType(resultBuffer.Length))
                        {
                            ForzaStatus = "Wrong buffer";
                            //connection.Send("new-data", $"buffer not the correct length. length is {resultBuffer.Length}");
                            //forzaStatus = "Buffer not correct: " + resultBuffer.Length.ToString();   
                            return;
                        }
                        //ForzaStatus = "Correct buffer";
                        data = ParseData(resultBuffer);


                    });
                    if (!data.IsRaceOn && !isPaused)
                    {
                        ForzaStatus = "Game paused";
                        receivingDataFlag = false;
                        isPaused = true;
                    }
                    else if (data.IsRaceOn && isPaused)
                    {
                        ForzaStatus = "Game unpaused";
                        isPaused = false;
                    }
                    if (!isPaused)
                    {
                        ForzaStatus = "Receiving data";
                        receivingDataFlag = true;
                        //ReceivingData();
                        //await Task.Delay(200);
                        //isRaceOn = data.IsRaceOn;
                    }
                    /*else
                    {
                        
                        receivingDataFlag = false;
                    }*/
                    if (ForzaRaceTime > 0 && ForzaCurrentLapTime != 0)
                    {
                        raceMode = true;
                    }
                    else
                    {
                        raceMode = false;
                    }

                }
            }
            catch (Exception)
            {
                throw;
            }

        }



        static DataPacket ParseData(byte[] packet)
        {
            DataPacket data = new DataPacket();

            // sled
            data.IsRaceOn = packet.IsRaceOn();
            data.TimestampMS = packet.TimestampMs();
            data.EngineMaxRpm = packet.EngineMaxRpm();
            data.EngineIdleRpm = packet.EngineIdleRpm();
            data.CurrentEngineRpm = packet.CurrentEngineRpm();
            data.AccelerationX = packet.AccelerationX();
            data.AccelerationY = packet.AccelerationY();
            data.AccelerationZ = packet.AccelerationZ();
            data.VelocityX = packet.VelocityX();
            data.VelocityY = packet.VelocityY();
            data.VelocityZ = packet.VelocityZ();
            data.AngularVelocityX = packet.AngularVelocityX();
            data.AngularVelocityY = packet.AngularVelocityY();
            data.AngularVelocityZ = packet.AngularVelocityZ();
            data.Yaw = packet.Yaw();
            data.Pitch = packet.Pitch();
            data.Roll = packet.Roll();
            data.NormalizedSuspensionTravelFrontLeft = packet.NormSuspensionTravelFl();
            data.NormalizedSuspensionTravelFrontRight = packet.NormSuspensionTravelFr();
            data.NormalizedSuspensionTravelRearLeft = packet.NormSuspensionTravelRl();
            data.NormalizedSuspensionTravelRearRight = packet.NormSuspensionTravelRr();
            data.TireSlipRatioFrontLeft = packet.TireSlipRatioFl();
            data.TireSlipRatioFrontRight = packet.TireSlipRatioFr();
            data.TireSlipRatioRearLeft = packet.TireSlipRatioRl();
            data.TireSlipRatioRearRight = packet.TireSlipRatioRr();
            data.WheelRotationSpeedFrontLeft = packet.WheelRotationSpeedFl();
            data.WheelRotationSpeedFrontRight = packet.WheelRotationSpeedFr();
            data.WheelRotationSpeedRearLeft = packet.WheelRotationSpeedRl();
            data.WheelRotationSpeedRearRight = packet.WheelRotationSpeedRr();
            data.WheelOnRumbleStripFrontLeft = packet.WheelOnRumbleStripFl();
            data.WheelOnRumbleStripFrontRight = packet.WheelOnRumbleStripFr();
            data.WheelOnRumbleStripRearLeft = packet.WheelOnRumbleStripRl();
            data.WheelOnRumbleStripRearRight = packet.WheelOnRumbleStripRr();
            data.WheelInPuddleDepthFrontLeft = packet.WheelInPuddleFl();
            data.WheelInPuddleDepthFrontRight = packet.WheelInPuddleFr();
            data.WheelInPuddleDepthRearLeft = packet.WheelInPuddleRl();
            data.WheelInPuddleDepthRearRight = packet.WheelInPuddleRr();
            data.SurfaceRumbleFrontLeft = packet.SurfaceRumbleFl();
            data.SurfaceRumbleFrontRight = packet.SurfaceRumbleFr();
            data.SurfaceRumbleRearLeft = packet.SurfaceRumbleRl();
            data.SurfaceRumbleRearRight = packet.SurfaceRumbleRr();
            data.TireSlipAngleFrontLeft = packet.TireSlipAngleFl();
            data.TireSlipAngleFrontRight = packet.TireSlipAngleFr();
            data.TireSlipAngleRearLeft = packet.TireSlipAngleRl();
            data.TireSlipAngleRearRight = packet.TireSlipAngleRr();
            data.TireCombinedSlipFrontLeft = packet.TireCombinedSlipFl();
            data.TireCombinedSlipFrontRight = packet.TireCombinedSlipFr();
            data.TireCombinedSlipRearLeft = packet.TireCombinedSlipRl();
            data.TireCombinedSlipRearRight = packet.TireCombinedSlipRr();
            data.SuspensionTravelMetersFrontLeft = packet.SuspensionTravelMetersFl();
            data.SuspensionTravelMetersFrontRight = packet.SuspensionTravelMetersFr();
            data.SuspensionTravelMetersRearLeft = packet.SuspensionTravelMetersRl();
            data.SuspensionTravelMetersRearRight = packet.SuspensionTravelMetersRr();
            data.CarOrdinal = packet.CarOrdinal();
            data.CarClass = packet.CarClass();
            data.CarPerformanceIndex = packet.CarPerformanceIndex();
            data.DrivetrainType = packet.DriveTrain();
            data.NumCylinders = packet.NumCylinders();

            // dash
            data.PositionX = packet.PositionX();
            data.PositionY = packet.PositionY();
            data.PositionZ = packet.PositionZ();
            data.Speed = packet.Speed();
            data.Power = packet.Power();
            data.Torque = packet.Torque();
            data.TireTempFl = packet.TireTempFl();
            data.TireTempFr = packet.TireTempFr();
            data.TireTempRl = packet.TireTempRl();
            data.TireTempRr = packet.TireTempRr();
            data.Boost = packet.Boost();
            data.Fuel = packet.Fuel();
            data.Distance = packet.Distance();
            data.BestLapTime = packet.BestLapTime();
            data.LastLapTime = packet.LastLapTime();
            data.CurrentLapTime = packet.CurrentLapTime();
            data.CurrentRaceTime = packet.CurrentRaceTime();
            data.Lap = packet.Lap();
            data.RacePosition = packet.RacePosition();
            data.Accelerator = packet.Accelerator();
            data.Brake = packet.Brake();
            data.Clutch = packet.Clutch();
            data.Handbrake = packet.Handbrake();
            data.Gear = packet.Gear();
            data.Steer = packet.Steer();
            data.NormalDrivingLine = packet.NormalDrivingLine();
            data.NormalAiBrakeDifference = packet.NormalAiBrakeDifference();

            return data;
        }

        static bool AdjustToBufferType(int bufferLength)
        {
            switch (bufferLength)
            {
                case 232: // FM7 sled
                    return false;
                case 311: // FM7 dash
                    FMData.BufferOffset = 0;
                    return true;
                case 324: // FH4
                    FMData.BufferOffset = 12;
                    return true;
                case 331: // FM8 dash
                    FMData.BufferOffset = 0;
                    return true;
                default:
                    return false;
            }
        }
    }
}
