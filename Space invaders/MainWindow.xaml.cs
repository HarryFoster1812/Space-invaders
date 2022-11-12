using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Activation;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Ink;
using System.Windows.Input;

using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;

using System.Windows.Threading; // Add support for threads in forms for timer
using System.Xml.Linq;

namespace Space_invaders

{
    class file_handler { 
        public string[,] readfile(string file1) {
            string[] filecontent = System.IO.File.ReadAllLines(file1);
            string[,] scores = new string[filecontent.Length,2];
            for(int i=0; i<filecontent.Length; i++) {
                scores[i,0] = filecontent[i].Split(' ')[0];
                scores[i, 1] = filecontent[i].Split(' ')[1];
            }
            
            return scores;

        }

        public void writefile(string file, string[,] values) {
            string[] text = new string[3];
            for (int i = 0; i < text.Length; i++)
            {
                text[i] = string.Concat(values[i,0], values[i,1]);
            }
            System.IO.File.WriteAllLines(file,text);
        }
    }


    public partial class MainWindow : Window

    {
        TabItem[] tabs;

        // Variables to store coordinates and status information about images on

        // form
        static int perrow = 10;
        static int rows = 3;
        private int laserbasexpos; // x coord of laser base
        private int laserxpos = -1000; // x and y coords of laser shot
        private int laserypos = -1000;
        private bool laseronscreen; // Flag to indicate if laser currently fired
        private bool missileonscreen;
        private int missilexpos = -1000;
        private int missileypos = -1000;
        private int invadermovement; // Number of pixels to move alien by
        private int level;
        private int lives = 4;
        // Images, to be dynamically created on the form

        private Image laserbase;
        private Image laser;
        private Image missile;
        private int counter = 0;
        private Image[] livespics;

        // ImageSources, into which the images to be displayed on the form will

        private ImageSource laserpic;
        private ImageSource invaderexplosionpic;
        private ImageSource laserbasepic;
        private ImageSource invaderposeApic;
        private ImageSource invaderposeB;
        private ImageSource missilepic;
        private Image[] invaders = new Image[perrow * rows];
        private int[,] invadersinfo = new int[perrow * rows, 3];
        private System.Media.SoundPlayer player = new System.Media.SoundPlayer();
        int destroy = -1;
        int score=0;
        Random random = new Random();
        file_handler highscorefile = new file_handler();
        const string path = "C:\\Users\\harry\\Documents\\Programming\\C#\\Space invaders\\Space invaders\\Sprites\\";
        DispatcherTimer dispatcherTimer = new DispatcherTimer();

        string[,] scores;

        // Function called when program is started

        public MainWindow()

        {
            InitializeComponent();

            scores = highscorefile.readfile(path + "highscores.txt");
            
            // Set defaults coordinates and other values
            Image[] values = { Life1, Life2, Life3 };
            TabItem[] values1 = { };
            livespics = values;
            laserbasexpos = 0;
            level = 1;
            invadermovement = 1; // 
            laseronscreen = false; // Laser not on screen

            // Load the PNG images that will be used into the game into
            // ImageSources


            laserbasepic = new BitmapImage(new Uri(path + "laserbase.png"));
            System.Media.SoundPlayer player = new System.Media.SoundPlayer();
            invaderposeApic = new BitmapImage(new Uri(path + "invaderposeA.png"));
            invaderposeB = new BitmapImage(new Uri(path + "invaderposeB.png"));
            invaderexplosionpic = new BitmapImage(new Uri(path + "explosion.png"));
            laserpic = new BitmapImage(new Uri(path + "laser.png"));
            missilepic = new BitmapImage(new Uri(path + "alienmissile.png"));

            // Create an image for the laser base on the canvas

            laserbase = new Image();
            laser = new Image();
            missile = new Image();
            missile.Height = 20;
            missile.Source = missilepic;

            missileonscreen = false;

            // Set the height and width of the image

            for (int i = 0; i < invaders.Length; i++)
            {

                invaders[i] = new Image();
                invaders[i].Source = invaderposeApic;
                invaders[i].Height = 30;
                invaders[i].Width = 30;
            }


            laserbase.Height = 30;

            laserbase.Width = 80;

            // Setup the image so that it displays the previously loaded laserbase

            // picture

            laserbase.Source = laserbasepic;


            laser.Source = laserpic;

            // Add the image to the canvas

       

            
            dispatcherTimer.Tick += new EventHandler(updatescreen);
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
  
            upadtescore();
        }

            public void upadtescore() {
            place1.Content = string.Concat(scores[0,0], scores[0, 1]);
            place2.Content = string.Concat(scores[1, 0], scores[1, 1]);
            place3.Content = string.Concat(scores[2, 0], scores[2, 1]);
        }
        public void newgame() {
        for (int i = 0; i < perrow; i++)
            {
                for (int j = 0; j < rows; j++)
                {

                    invadersinfo[i + j * perrow, 0] = ((int)invaders[0].Width + 10) * i;
                    invadersinfo[i + j * perrow, 1] = ((int)invaders[0].Height + 10) * j;
                    invadersinfo[i + j * perrow, 2] = 0;
                    invaders[i + j * perrow].Source = invaderposeApic;
                    invaders[i + j * perrow].Visibility = Visibility.Visible;
                    if (lives <= 0) { 
                        GameCanvas.Children.Add(invaders[i + j * perrow]);
                    }
                    Canvas.SetLeft(invaders[i + j * perrow], ((int)invaders[0].Width + 10) * i);

                    Canvas.SetTop(invaders[i + j * perrow], ((int)invaders[0].Height + 10) * j);
                }
            }
            if (lives <= 0) { 
            GameCanvas.Children.Add(missile);
            GameCanvas.Children.Add(laserbase);
            Canvas.SetLeft(laserbase, laserbasexpos);

            Canvas.SetTop(laserbase, GameCanvas.Height - 80);
            GameCanvas.Children.Add(laser);
            }


            if (lives >= 1)
            {
                level += 1;
                invadermovement = (2*level - 2);
                PLevel.Content = level.ToString();
            }

            else {
                level = 1;
                invadermovement = 1;
                PLevel.Content = level.ToString();
                for (int i = 0; i < 3; i++) {
                    livespics[i].Visibility = Visibility.Visible;
                }
                lives = 4;
                score = 0;
                PScore.Content = score.ToString();


            }
            destroy = -1;
            dispatcherTimer.Start();
        }
        public void removeobjects() { 
            for(int i=0; i < invaders.Length;i++) { 
            
                GameCanvas.Children.Clear();
                dispatcherTimer.Stop();
            }
        }


        public void endgame(int case1)
        {
            // do some logic to check high score, also add some logic to do like some high score stuff
            if (case1 == 1)
            {
                newgame();
            }
            else if (case1 == 2)
            {
                removeobjects();
                if (score > int.Parse(scores[0,1])){
                    scores[0, 0] = name.Text;
                    scores[0, 1] = score.ToString();

                }
                else if(score > int.Parse(scores[1, 1]))
                {
                    scores[1, 0] = name.Text;
                    scores[1,1] = score.ToString();
                
                }
                else if(score > int.Parse(scores[2, 1]))
                {
                    scores[2, 0] = name.Text;
                    scores[2,1] = score.ToString();
                
                }
                highscorefile.writefile(path + "highscores.txt", scores);
                control.SelectedIndex = 0;
            }
            else {
                removeobjects();
                if (score > int.Parse(scores[0, 1]))
                {
                    scores[0, 0] = name.Text;
                    scores[0, 1] = score.ToString();

                }
                else if (score > int.Parse(scores[1, 1]))
                {
                    scores[1, 0] = name.Text;
                    scores[1, 1] = score.ToString();

                }
                else if (score > int.Parse(scores[2, 1]))
                {
                    scores[2, 0] = name.Text;
                    scores[2, 1] = score.ToString();

                }
                lives = 0;
                highscorefile.writefile(path + "highscores.txt", scores);
                control.SelectedIndex = 0;
            }
            
          
        }

        public void updatescreen(object sender, EventArgs e) {

            if (invadersinfo[perrow * rows - 1, 1] >= (GameCanvas.Height - 80)) {
                endgame(3);
            }
            counter++;
            if (counter % 2 == 0) {
                for (int i = 0; i < invaders.Length; i++) {
                    invaders[i].Source = invaderposeApic;
                }
            }
            else {
                for (int i = 0; i < invaders.Length; i++) {
                    invaders[i].Source = invaderposeB;
                }
            }

            if (invadersinfo[perrow * rows - 1, 0] >= GameCanvas.Width - (invaders[0].Width + 20) || invadersinfo[0, 0] < 0)
            {
                invadermovement = 0 - invadermovement;

                for (int i = 0; i < invaders.Length; i++) {
                    invadersinfo[i, 0] += invadermovement;
                    invadersinfo[i, 1] += (int)invaders[0].Height;
                    Canvas.SetLeft(invaders[i], invadersinfo[i, 0]);
                    Canvas.SetTop(invaders[i], invadersinfo[i, 1]);
                }
            }
            else {
                for (int i = 0; i < invaders.Length; i++) {
                    invadersinfo[i, 0] += invadermovement;
                    Canvas.SetLeft(invaders[i], invadersinfo[i, 0]);
                }
            }
            
            if (destroy != -1)
            {
                invaders[destroy].Visibility = Visibility.Hidden;
                destroy = -1;
            }

            if (laseronscreen == true || missileonscreen==true)
            {
                if (laserypos <= 0)
                {
                    laseronscreen = false;
                    laserxpos = -1000;
                    laserypos = -1000;
                    laser.Visibility = Visibility.Hidden;
                }
                if (missileypos > GameCanvas.Height-70) {
                    missileonscreen = false;
                    missilexpos = -1000;
                    missileypos = -1000;
                    missile.Visibility = Visibility.Hidden;
                }

                checkhit();

                laserypos -= 25;
                missileypos += 25;

                Canvas.SetTop(laser, laserypos);
                Canvas.SetTop(missile, missileypos);
                
            }
            
            
            if (missileonscreen == false)
            {
                missileonscreen = true;
                bool flag = false;
                while (flag == false)
                {
                    int random1 = random.Next(perrow);
                    
                    for(int i=1; i < rows+1; i++) { 
                        if (invadersinfo[random1 * rows + (rows-i), 2] != 1) {
                        missile.Visibility = Visibility.Visible;
                        missilexpos = invadersinfo[random1 * rows + (rows-i), 0] + ((int)invaders[0].Width);
                        missileypos = invadersinfo[random1 * rows +(rows-i), 1] + ((int)invaders[0].Height);
                        Canvas.SetLeft(missile, missilexpos);
                        Canvas.SetTop(missile, missileypos);
                        flag = true;
                            break;
                        }
                    
                    }
                }

            }

            


        }
        // Function to check if the laser shot has hit the alien invader

        private void checkhit()
        { 
            if (laseronscreen == true)
            {
                if (laserypos <= invadersinfo[perrow * rows - 1, 1] + 30)
                {
                    for (int i = 0; i < invaders.Length; i++)
                    {
                        if (invadersinfo[i,2] == 0)
                        {
                            if ((laserxpos >= invadersinfo[i, 0] && laserxpos <= invadersinfo[i, 0] + invaders[0].Width) && (laserypos >= invadersinfo[i, 1] && laserypos <= invadersinfo[i, 1] + 30))
                            {
                                laseronscreen = false;

                                laser.Visibility = Visibility.Hidden;
                                laserxpos = -1000;
                                laserypos = -1000;
                                invaders[i].Source = invaderexplosionpic;
                                destroy = i;
                                invadersinfo[i, 2] = 1;
                                score += 10;
                                PScore.Content = score.ToString();
                                player.SoundLocation = path + "invaderkilled.WAV";
                                player.Play();
                                if (score >= (perrow*rows*level*10))

                                { endgame(1); }

                            }
                        }
                    }
                }
            }
            if (missileonscreen == true) {
                if ((missilexpos >= laserbasexpos && missilexpos <= laserbasexpos + laserbase.Width) && ((missileypos+missile.Height) >= (GameCanvas.Height - 80))) {
                    
                    lives -= 1;
                    player.SoundLocation = path + "explosion.WAV";
                    player.Play();
                    if (lives <= 0)
                    {
                        endgame(2);
                    }
                    else { 
                    livespics[3 - lives].Visibility = Visibility.Hidden;
                    }
                    missileonscreen = false;
                    missilexpos = -1000;
                    missileypos = -1000;
                    missile.Visibility = Visibility.Hidden;
                    
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (control.SelectedIndex == 1) { 
                if (e.Key == Key.A || e.Key == Key.Left)
                {
                    if ((laserbasexpos - 10) > 0)
                    {
                        laserbasexpos -= 10;
                        Canvas.SetLeft(laserbase, laserbasexpos);
                    }
                }
                else if (e.Key == Key.D || e.Key == Key.Right)
                {
                    if ((laserbasexpos + 10) < (GameCanvas.Width-laserbase.Width)) {
                        laserbasexpos += 10;

                        Canvas.SetLeft(laserbase, laserbasexpos);
                    }
                
                }
                else if (e.Key == Key.Enter ||e.Key == Key.Space) {

                   

                    if (laseronscreen == false){ 
                         player.SoundLocation = path + "shoot.WAV";
                        player.Play(); 
                        laserypos = (int)(GameCanvas.Height) - 120;
                        laser.Visibility = Visibility.Visible;
                        laseronscreen = true;
                        laserxpos = laserbasexpos+(int)(laserbase.Width/2);
                        Canvas.SetTop(laser, laserypos);
                        Canvas.SetLeft(laser, laserxpos);
                       }
                 }
           }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Name == "Start")
            {
                control.SelectedIndex = 1;
                lives = 0;
                newgame();
            }
            else if (((Button)sender).Name == "highscore")
            {
                control.SelectedIndex = 2;
                upadtescore();
            }
            else {
                control.SelectedIndex = 0;
            }
        }

        private void keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right || e.Key == Key.Left) {
                e.Handled = true;
            }
        }
    }
}