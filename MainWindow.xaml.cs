//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Kinect.BodyStream
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        private List<Card> cards = new List<Card>();
        


        private DepthFrameReader depthFrameReader;
        // Color Data Structures
        private byte[] depthPixels = null;
        private WriteableBitmap bitmap = null;
        private const int BytesPerPixel = 4;

        private ushort[] depthFrameData;

        //gesture detectors and event raiser
        private GestureResultView result;
        private GestureDetector detector;

        private DateTime prevTime;
        private DateTime currTime;


        private int cartas = 0;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            

            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();


            // get the depth (display) extents
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            //depth reader and data structures
            depthFrameReader = kinectSensor.DepthFrameSource.OpenReader();

            FrameDescription depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
            this.depthFrameData = new ushort[depthFrameDescription.Width *
                                    depthFrameDescription.Height];

            this.depthPixels =
                new byte[depthFrameDescription.Width *
                  depthFrameDescription.Height * BytesPerPixel];
            // create the bitmap to display
            this.bitmap =
                new WriteableBitmap(depthFrameDescription.Width,
                depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            // open the sensor
            this.kinectSensor.Open();

            // use the window object as the view model in this simple example
            this.DataContext = this;

            result = new GestureResultView(0, "", false, false, 0.0f);
            detector = new GestureDetector(this.kinectSensor, result);
            result.PropertyChanged += GestureResult_PropertyChanged;

            
            prevTime = DateTime.Now;

            // initialize the components (controls) of the window
            this.InitializeComponent();
            this.elegirMazo();
        }

       
        
        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Execute start up tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {              
                
                for (int i = 0; i < bodies.Length; ++i)
                {
                    if (bodies[i].IsTracked == true)
                    {
                        this.detector.TrackingId = bodies[i].TrackingId;
                        this.detector.IsPaused = false;
                    }
                }
                

            }
        }
        void GestureResult_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            GestureResultView result = sender as GestureResultView;

            if (e.PropertyName.Equals("Detected") && result.Detected && (!result.GestureName.Equals("")))
            {
                currTime = DateTime.Now;
                TimeSpan diff = currTime - prevTime;
                prevTime = currTime;

                if (diff.TotalSeconds > 1)
                {
                    if (result.GestureName.Equals("brazoArribaDerecha"))
                    {
                        displayCard("topRight");
                        Console.WriteLine("ARRIBA_derecha");
                    }
                    else
                    if (result.GestureName.Equals("brazoArribaIzquierda_Left"))
                    {
                        displayCard("topLeft");
                        Console.WriteLine("ARRIBA_izquierda");
                    }
                    else
                    if (result.GestureName.Equals("brazoCentro_Right"))
                    {
                        displayCard("bottomRight");
                        Console.WriteLine("ABAJO DERECHA");
                    }
                    else
                    if (result.GestureName.Equals("BrazoCentroIzquierda_Left"))
                    {
                        displayCard("bottomLeft");
                        Console.WriteLine("Abajo_izquierda");
                    }
                    else
                    if (result.GestureName.Equals("brazosArriba"))
                    {
                        displayCard("topCenter");
                        Console.WriteLine("ARRIBA_centro");
                    }
                    else
                    if (result.GestureName.Equals("brazosCentro"))
                    {
                        displayCard("bottomCenter");
                        Console.WriteLine("abajo centro");
                    }

                }

            }
        }

        void displayCard(string position)
        {
            Card selectedCard = null;

            switch(position) {
                case "topLeft":
                    selectedCard = this.cards.Find(c => c.name == "topLeft");
                    break;
                case "bottomLeft":
                    selectedCard = this.cards.Find(c => c.name == "bottomLeft");
                    break;
                case "topRight":
                    selectedCard = this.cards.Find(c => c.name == "topRight");
                    break;
                case "bottomRight":
                    selectedCard = this.cards.Find(c => c.name == "bottomRight");
                    break;
                case "topCenter":
                    selectedCard = this.cards.Find(c => c.name == "topCenter");
                    break;
                case "bottomCenter":
                    selectedCard = this.cards.Find(c => c.name == "bottomCenter");
                    break;
            }
            
            if (selectedCard != null && !selectedCard.opened)
            {

                if(cartas == 1)
                {
                    checkCondition(selectedCard);
                }

                if (cartas == 2)
                {
                    Console.WriteLine(cartas);

                    cartas = 0;
                    foreach (Card c in cards)
                    {
                        if (!c.matched)
                            c.ocultar();
                        this.avisoPareja.Visibility = Visibility.Hidden;
                        this.avisoNoPareja.Visibility = Visibility.Hidden;
                    }

                }
                cartas++;
                selectedCard.mostrar();
                
                
            }
            
        }


        private void checkCondition(Card selectedCard)
        {
            
            if(this.cards.Find(c => c.match == selectedCard.id).opened)
            {
                this.cards.Find(c => c.match == selectedCard.id).matched = true;
                this.cards.Find(c => c.id == selectedCard.id).matched = true;
                this.avisoPareja.Visibility = Visibility.Visible;

            }
            else
            {
                this.avisoPareja.Visibility = Visibility.Hidden;
                this.avisoNoPareja.Visibility = Visibility.Visible;
            }

            if (this.cards.Find(c => !c.matched) == null)
            {
                this.avisoFin.Visibility = Visibility.Visible;
                this.avisoPareja.Visibility = Visibility.Hidden;
                this.avisoNoPareja.Visibility = Visibility.Hidden;
                Console.WriteLine("FIN DE PARTIDA");
            }

        }

        private void elegirMazo()
        {
            Random rnd = new Random();
            int rdm = rnd.Next(0, 8);

            switch (rdm)
            {
                case 0:
                    createCards();
                    break;
                case 1:
                    createCardsA();
                    break;
                case 2:
                    createCardsB();
                    break;
                case 3:
                    createCardsC();
                    break;
                case 4:
                    createCardsD();
                    break;
                case 5:
                    createCardsE();
                    break;
                case 6:
                    createCardsF();
                    break;
                case 7:
                    createCardsG();
                    break;
                case 8:
                    createCardsH();
                    break;
            }
        }
        private void createCards()
        {
            this.cards.Add(new Card(1, 3, "topLeft", this.top_izq, this.top_izq_revealed));
            this.cards.Add(new Card(2, 4, "topRight", this.top_der, this.top_der_revealed));
            this.cards.Add(new Card(3, 1, "bottomLeft", this.bottom_izq, this.bottom_izq_revealed));
            this.cards.Add(new Card(4, 2, "bottomRight", this.bottom_der, this.bottom_der_revealed));
            this.cards.Add(new Card(5, 6, "topCenter", this.top_cent, this.top_cent_revealed));
            this.cards.Add(new Card(6, 5, "bottomCenter", this.bot_cent, this.bot_cent_revealed));
        }

        private void createCardsA()
        {

            this.top_izq_revealed.Source = this.pica.Source;
            this.top_cent_revealed.Source = this.pica.Source;
            this.top_der_revealed.Source = this.corazon.Source;
            this.bottom_izq_revealed.Source = this.corazon.Source;
            this.bot_cent_revealed.Source = this.trebol.Source;
            this.bottom_der_revealed.Source = this.trebol.Source;
            this.cards.Add(new Card(1, 2, "topLeft", this.top_izq, this.top_izq_revealed));
            this.cards.Add(new Card(2, 1, "topCenter", this.top_cent, this.top_cent_revealed));
            this.cards.Add(new Card(3, 4, "topRight", this.top_der, this.top_der_revealed));
            this.cards.Add(new Card(4, 3, "bottomLeft", this.bottom_izq, this.bottom_izq_revealed));
            this.cards.Add(new Card(5, 6, "bottomCenter", this.bot_cent, this.bot_cent_revealed));
            this.cards.Add(new Card(6, 5, "bottomRight", this.bottom_der, this.bottom_der_revealed));
            
            
        }

        private void createCardsB()
        {

            this.top_izq_revealed.Source = this.pica.Source;
            this.top_cent_revealed.Source = this.trebol.Source;
            this.top_der_revealed.Source = this.corazon.Source;
            this.bottom_izq_revealed.Source = this.trebol.Source;
            this.bot_cent_revealed.Source = this.pica.Source;
            this.bottom_der_revealed.Source = this.corazon.Source;
            
            this.cards.Add(new Card(1, 5, "topLeft", this.top_izq, this.top_izq_revealed));
            this.cards.Add(new Card(2, 4, "topCenter", this.top_cent, this.top_cent_revealed));
            this.cards.Add(new Card(3, 6, "topRight", this.top_der, this.top_der_revealed));
            this.cards.Add(new Card(4, 2, "bottomLeft", this.bottom_izq, this.bottom_izq_revealed));
            this.cards.Add(new Card(5, 1, "bottomCenter", this.bot_cent, this.bot_cent_revealed));
            this.cards.Add(new Card(6, 3, "bottomRight", this.bottom_der, this.bottom_der_revealed));
            
            
        }
        private void createCardsC()
        {
            this.top_izq_revealed.Source = this.corazon.Source;
            this.top_cent_revealed.Source = this.pica.Source;
            this.top_der_revealed.Source = this.trebol.Source;
            this.bottom_izq_revealed.Source = this.pica.Source;
            this.bot_cent_revealed.Source = this.trebol.Source;
            this.bottom_der_revealed.Source = this.corazon.Source;

            this.cards.Add(new Card(1, 6, "topLeft", this.top_izq, this.top_izq_revealed));
            this.cards.Add(new Card(2, 4, "topCenter", this.top_cent, this.top_cent_revealed));
            this.cards.Add(new Card(3, 5, "topRight", this.top_der, this.top_der_revealed));
            this.cards.Add(new Card(4, 2, "bottomLeft", this.bottom_izq, this.bottom_izq_revealed));
            this.cards.Add(new Card(5, 3, "bottomCenter", this.bot_cent, this.bot_cent_revealed));
            this.cards.Add(new Card(6, 1, "bottomRight", this.bottom_der, this.bottom_der_revealed));
            
            
        }

        private void createCardsD()
        {

            this.top_izq_revealed.Source = this.trebol.Source;
            this.top_cent_revealed.Source = this.pica.Source;
            this.top_der_revealed.Source = this.trebol.Source;
            this.bottom_izq_revealed.Source = this.pica.Source;
            this.bot_cent_revealed.Source = this.corazon.Source;
            this.bottom_der_revealed.Source = this.corazon.Source;

            this.cards.Add(new Card(1, 3, "topLeft", this.top_izq, this.top_izq_revealed));
            this.cards.Add(new Card(2, 4, "topCenter", this.top_cent, this.top_cent_revealed));
            this.cards.Add(new Card(3, 1, "topRight", this.top_der, this.top_der_revealed));
            this.cards.Add(new Card(4, 2, "bottomLeft", this.bottom_izq, this.bottom_izq_revealed));
            this.cards.Add(new Card(5, 6, "bottomCenter", this.bot_cent, this.bot_cent_revealed));
            this.cards.Add(new Card(6, 5, "bottomRight", this.bottom_der, this.bottom_der_revealed));
        }


        private void createCardsE()
        {

            this.top_izq_revealed.Source = this.corazon.Source; 
            this.top_cent_revealed.Source = this.trebol.Source; 
            this.top_der_revealed.Source = this.pica.Source; 
            this.bottom_izq_revealed.Source = this.corazon.Source;
            this.bot_cent_revealed.Source = this.trebol.Source;
            this.bottom_der_revealed.Source = this.pica.Source;

            this.cards.Add(new Card(1, 4, "topLeft", this.top_izq, this.top_izq_revealed));
            this.cards.Add(new Card(2, 5, "topCenter", this.top_cent, this.top_cent_revealed));
            this.cards.Add(new Card(3, 6, "topRight", this.top_der, this.top_der_revealed));
            this.cards.Add(new Card(4, 1, "bottomLeft", this.bottom_izq, this.bottom_izq_revealed));
            this.cards.Add(new Card(5, 2, "bottomCenter", this.bot_cent, this.bot_cent_revealed));
            this.cards.Add(new Card(6, 3, "bottomRight", this.bottom_der, this.bottom_der_revealed));
        }

        private void createCardsF()
        {

            this.top_izq_revealed.Source = this.trebol.Source; 
            this.top_cent_revealed.Source = this.corazon.Source;
            this.top_der_revealed.Source = this.corazon.Source; 
            this.bottom_izq_revealed.Source = this.pica.Source;
            this.bot_cent_revealed.Source = this.trebol.Source;
            this.bottom_der_revealed.Source = this.pica.Source;

            this.cards.Add(new Card(1, 5, "topLeft", this.top_izq, this.top_izq_revealed));
            this.cards.Add(new Card(2, 3, "topCenter", this.top_cent, this.top_cent_revealed));
            this.cards.Add(new Card(3, 2, "topRight", this.top_der, this.top_der_revealed));
            this.cards.Add(new Card(4, 6, "bottomLeft", this.bottom_izq, this.bottom_izq_revealed));
            this.cards.Add(new Card(5, 1, "bottomCenter", this.bot_cent, this.bot_cent_revealed));
            this.cards.Add(new Card(6, 4, "bottomRight", this.bottom_der, this.bottom_der_revealed));
        }

        private void createCardsG()
        {

            this.top_izq_revealed.Source = this.corazon.Source;  
            this.top_cent_revealed.Source = this.corazon.Source;
            this.top_der_revealed.Source = this.pica.Source;  
            this.bottom_izq_revealed.Source = this.trebol.Source;
            this.bot_cent_revealed.Source = this.pica.Source;  
            this.bottom_der_revealed.Source = this.corazon.Source;

            this.cards.Add(new Card(1, 2, "topLeft", this.top_izq, this.top_izq_revealed));
            this.cards.Add(new Card(2, 1, "topCenter", this.top_cent, this.top_cent_revealed));
            this.cards.Add(new Card(3, 5, "topRight", this.top_der, this.top_der_revealed));
            this.cards.Add(new Card(4, 6, "bottomLeft", this.bottom_izq, this.bottom_izq_revealed));
            this.cards.Add(new Card(5, 3, "bottomCenter", this.bot_cent, this.bot_cent_revealed));
            this.cards.Add(new Card(6, 4, "bottomRight", this.bottom_der, this.bottom_der_revealed));
        }
        private void createCardsH()
        {

            this.top_izq_revealed.Source = this.trebol.Source;
            this.top_cent_revealed.Source = this.corazon.Source;
            this.top_der_revealed.Source = this.pica.Source;
            this.bottom_izq_revealed.Source = this.corazon.Source;
            this.bot_cent_revealed.Source = this.pica.Source;
            this.bottom_der_revealed.Source = this.trebol.Source;

            this.cards.Add(new Card(1, 6, "topLeft", this.top_izq, this.top_izq_revealed));
            this.cards.Add(new Card(2, 4, "topCenter", this.top_cent, this.top_cent_revealed));
            this.cards.Add(new Card(3, 5, "topRight", this.top_der, this.top_der_revealed));
            this.cards.Add(new Card(4, 2, "bottomLeft", this.bottom_izq, this.bottom_izq_revealed));
            this.cards.Add(new Card(5, 3, "bottomCenter", this.bot_cent, this.bot_cent_revealed));
            this.cards.Add(new Card(6, 1, "bottomRight", this.bottom_der, this.bottom_der_revealed));
        }
    }
}
