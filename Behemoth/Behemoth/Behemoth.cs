using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocode;
using System.IO;
using System.Security.Permissions;

namespace Karol
{
    public class Behemoth : Robot
    {
        //E_Direction _direction;

        string _LOG_PATH = @"C:\Users\Karlik\Documents\programowanie\walka robotow\Behemoth\Behemoth\bin\Release\log.txt";//@"C:\robocode\moje_logi\trafienia.txt";
        int _ESCAPE_FROM_BULLET_AHEAD_OR_BACK = 0; //0 - prosto, 1 - tył. określa jak ma uciekać po trafieniu przez wroga
        int _ESCAPE_FROM_WALL_AHEAD_OR_BACK = 0; //0 - prosto, 1 - tył. określa jak ma uciekać spod ściany
        bool _ACTION_START = false; //jesli true to znaczy, ze nie musi już lecieć w pętli bo jest strzelanina
        bool _IS_TARGET = false;    //true jesli wróg namierzony

        public override void Run()
        {
            
            //inicjalizacja
            //_direction = E_Direction.Ahead;
            //TurnLeft(Heading - 90);
            //TurnGunRight(90);

            while (true)
            {
                if (_ACTION_START == false)
                {
                    TurnGunRight(40);
                    Ahead(25);
                    //TurnRight(40);
                }
                else
                {
                    //int dummmmm = 0;
                    TurnRadarRight(15);
                    Scan();
                }

                // Move our robot 5000 pixels ahead
                //Ahead(5000);

                // Turn the robot 90 degrees
                //TurnRight(90);
                
                // Our robot will move along the borders of the battle field
                // by repeating the above two statements.
            }

            //base.Run();
        }

        /// <summary>
        /// zdarzenie jak spotkam innego robota na swojej drodze
        /// </summary>
        /// <param name="e"></param>
        // Robot event handler, when the robot sees another robot
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            Stop(true);
            //if (!_IS_TARGET)
            //{
                double offset = 0;

                if (e.Bearing < 0)
                {
                    offset = 360 + e.Bearing + Heading;
                }
                else
                {
                    offset = e.Bearing + Heading;
                }

                //if(this.Heading>e.Bearing)
                //{
                //    offset = Heading - (Heading - e.Bearing); //todo -180 ... 180
                //}
                //else
                //{
                //    offset = e.Bearing - (e.Bearing - Heading);
                //}

                //offset = -this.Heading + e.Bearing;
                //if (offset < 0)
                //    offset = 360 + offset;
                SetGunForHit(offset);    //e.Bearing to jest relatywna pozyca wroga do mojego czołgu
            //}
            //else
            //{
                if (e.Distance < 400)
                {
                    Fire(Rules.MAX_BULLET_POWER);
                }
                else
                {
                    Fire(1);
                }
            //}

            // resume ?
        }

        /// <summary>
        /// nietrafiony pocisk
        /// </summary>
        /// <param name="evnt"></param>
        public override void OnBulletMissed(BulletMissedEvent e)
        {
            //double gunFactor = e.Bullet.Heading - GunHeading;
            //if (e.Bullet.Heading == GunHeading)
            //{
                _IS_TARGET = false;
            //    Scan();
            //}
        }

        /// <summary>
        /// rtafiłem robota
        /// </summary>
        /// <param name="evnt"></param>
        public override void OnBulletHit(BulletHitEvent e)
        {
            if (!_ACTION_START)
            {
                _ACTION_START = true;
                SetTankForCorner(e.Bullet.Heading);
            }
            
            CustomLog("JA TRAFIŁEM. power trafienia: " + e.Bullet.Power.ToString("F2") + " heat: " + GetGunHeat().ToString("F2") +" moja enerdia: "+Energy.ToString("F2") + Environment.NewLine);

            //jak namiar jest na to samo miejsce co trafienie to strzel jeszcze raz z największą mocą
            double gunFactor = e.Bullet.Heading - GunHeading;
            //if (_IS_TARGET)
            if ((gunFactor > -1 && gunFactor < 1) && _IS_TARGET)
            {
                if (GetGunHeat() == 0)
                    Fire(Rules.MAX_BULLET_POWER);
            }
            else
                SetGunForHit(e.Bullet.Heading);
        }
        
        //todo: refaktoryzacja

        /// <summary>
        /// ustawia czołg prostopadle do linni ognia (żeby uciekać)
        /// </summary>
        /// <param name="bulletHeading"></param>
        private void SetTankForCorner(double bulletHeading)
        {
            double offsetL = -this.Heading + bulletHeading;// - (360 + bulletHeading);

            while (Math.Abs(offsetL) > 360)
                if (offsetL < 0)
                    offsetL += 360;
                else
                    offsetL -= 360;

            if (offsetL < 0)
                offsetL = 360 + offsetL;

            if ((offsetL >= 0 && offsetL < 2) || (offsetL > 358 && offsetL <= 360))
            {
                return;
            }

            if (offsetL >= 0 && offsetL < 180)
                TurnRight(offsetL - 90);
            else
                TurnLeft(-offsetL - 90);

            //double offset = 0;
            //if (this.Heading > 180)
            //    TurnRight(360 - this.Heading);
            //else
            //    TurnLeft(this.Heading);
            //if (bulletHeading > 180)
            //    TurnLeft(360-bulletHeading - 90);
            //else
            //    TurnRight(bulletHeading - 90);

        }

        /// <summary>
        /// ustawia broń na odpowiednią pozycję
        /// </summary>
        private void SetGunForHit(double bulletHeading)
        {
            double offsetL = -this.GunHeading + bulletHeading;// - (360 + bulletHeading);

            while (Math.Abs(offsetL) > 360)
                if (offsetL < 0)
                    offsetL += 360;
                else
                    offsetL -= 360;

            if (offsetL < 0)
                offsetL = 360 + offsetL;

            //lufa już jest dobrze ustawiona
            if((offsetL>=0 && offsetL<2) || (offsetL > 358 && offsetL <= 360))
            {
                _IS_TARGET = true;
                return;
            }

            if (offsetL >= 0 && offsetL < 180)
                TurnGunRight(offsetL);
            else
                TurnGunLeft(360 - offsetL);
                //TurnGunLeft(-offsetL);

            //zapas
            //double offsetR = 0, offsetL = 0;
            //if (this.GunHeading > 180)
            //{
            //    offsetR = 360 - this.GunHeading;
            //    if (offsetR >= 0 && offsetR < 180)
            //        offsetL = offsetR + 180;
            //    else
            //        offsetL = offsetR - 180;
            //}
            //else
            //{
            //    offsetL = this.Heading;
            //    if (offsetL >= 0 && offsetL < 180)
            //        offsetR = offsetL + 180;
            //    else
            //        offsetR = offsetL - 180;
            //}
            //if (bulletHeading > 180)
            //    TurnGunLeft(offsetL + 360 - bulletHeading);
            //else
            //    TurnGunRight(offsetR + bulletHeading);

            //zapas
            //if (this.GunHeading > 180)
            //    //offset = 360 - this.GunHeading;
            //    TurnGunRight(360 - this.GunHeading);
            //else
            //    //offset = this.Heading;
            //    TurnGunLeft(this.GunHeading);
            //if (bulletHeading > 180)
            //    TurnGunLeft(360 - bulletHeading);
            //else
            //    TurnGunRight(bulletHeading);

            _IS_TARGET = true;
        }

        /// <summary>
        /// zdarzenie jak zostanę trafiony
        /// </summary>
        /// <param name="evnt"></param>
        public override void OnHitByBullet(HitByBulletEvent e)
        {
            if (!_ACTION_START)
            {
                _ACTION_START = true;
                SetTankForCorner(e.Heading + 180);
            }

            CustomLog("TRAFILI MNIE. bearing: " + e.Bearing.ToString("F2") + " heading: " + e.Heading.ToString("F2") 
                + " bearing radian: " + e.BearingRadians.ToString("F2") + " heading radian: " + e.HeadingRadians.ToString("F2"));

            
            //ustaw broń do strzelania
            //TurnRadarRight(360);
            SetGunForHit(e.Heading + 180);
            Fire(1);

            if (_ESCAPE_FROM_BULLET_AHEAD_OR_BACK == 0)
            {
                Back(200);
                _ESCAPE_FROM_BULLET_AHEAD_OR_BACK = 1;
            }
            else
            {
                Ahead(200);
                _ESCAPE_FROM_BULLET_AHEAD_OR_BACK = 0;
            }

            _IS_TARGET = false;
            Scan();

        }

        /// <summary>
        /// zdarzenie uderzenia w ścianę
        /// </summary>
        /// <param name="evnt"></param>
        public override void OnHitWall(HitWallEvent e)
        {
            _IS_TARGET = false;

            //określa czy trzeba spod ściany odjechać przodem czy tyłem
            if ((e.Bearing >= -90 && e.Bearing <= 0) || (e.Bearing >= 0 && e.Bearing <= 90))
                Back(200);
            else
                Ahead(200);

            TurnRight(15);

            //todo: rozwiązać problem, że jak do tyłu jest od razu ściana to sprobuje potem do przodu jechać (bo tak to sie moze zaklinować)
            //Back(300);
        }

        

        #region tools
        private double GetGunHeat()
        {
            return this.GunHeat;
        }

        /// <summary>
        /// zapisuje log do pliku
        /// </summary>
        /// <param name="msg"></param>
        //[System.Security.SecurityCritical]
        private void CustomLog(string msg)
        {

            
            try
            {
                //FileIOPermission p = new FileIOPermission(FileIOPermissionAccess.AllAccess, _LOG_PATH);
                //p.Demand();
                //p.Assert();
                //File.AppendAllText(_LOG_PATH, msg);
            }
            catch (Exception ex)
            {

            }
        }
        #endregion
    }
}
