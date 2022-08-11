using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace WinFormsApp2
{
    public partial class Form3 : Form
    {
        double test1;
        double test2;
        public Form3(Collection<DataPoint> brake_line)
        {
            InitializeComponent();
            var braking_line_model = new PlotModel();
            var braking_line = new LineSeries();
            var x_axis = new LinearAxis();
            var y_axis = new LinearAxis();
            x_axis.Position = AxisPosition.Bottom;
            y_axis.Position = AxisPosition.Left;
            x_axis.Key = "x";
            y_axis.Key = "y";
            braking_line.Points.AddRange(brake_line);
            braking_line.XAxisKey = "x";
            braking_line.YAxisKey = "y";
            braking_line_model.Axes.Add(x_axis);
            braking_line_model.Axes.Add(y_axis);
            braking_line_model.Series.Add(braking_line);
            car_braking_line.Model = braking_line_model;
        }

        private void Form3_Load(object sender, EventArgs e)
        {


        }

        public void Update(Collection<DataPoint> brake_line, double add_x, double add_y, double rotation_angle, float veh_speed)
        {

            var braking_line_model = new PlotModel();
            var braking_line = new LineSeries();
            var corner_line = new LineSeries();
            var last_point = new ScatterSeries();
            var x_axis = new LinearAxis();
            var y_axis = new LinearAxis();
            x_axis.Position = AxisPosition.Bottom;
            y_axis.Position = AxisPosition.Left;
            x_axis.Key = "x";
            y_axis.Key = "y";
            braking_line.StrokeThickness = 8;
            braking_line.XAxisKey = "x";
            braking_line.YAxisKey = "y";
            braking_line.Points.AddRange(brake_line);
            braking_line.Color = OxyColors.Green;
            corner_line.Color = OxyColors.Orange;

            x_axis.MajorGridlineStyle = LineStyle.Solid;
            x_axis.MinorGridlineStyle = LineStyle.Solid;
            y_axis.MajorGridlineStyle= LineStyle.Solid;
            y_axis.MinorGridlineStyle= LineStyle.Solid;

            braking_line_model.Axes.Add(x_axis);
            braking_line_model.Axes.Add(y_axis);


            try
            {
                Collection<DataPoint> corner_points = new Collection<DataPoint>();
                int steps = (int)((360 * Math.PI / 180) / rotation_angle);
                corner_points.Add(brake_line.Last());
                var aaaa = new ScatterPoint(brake_line.Last().X, brake_line.Last().Y);
                aaaa.Size = 4;
                last_point.MarkerType = MarkerType.Circle;
                last_point.MarkerFill = OxyColors.Green;
                last_point.Points.Add(aaaa);
                Vector speed = new Vector { X = add_x, Y = add_y };
                var speed_vector = Math.Atan2(speed.Y, speed.X);
                for (int i = 0; i <= Math.Abs(steps); i++)
                {
                    speed_vector = speed_vector + rotation_angle;
                    speed = new Vector { X = Math.Cos(speed_vector), Y = Math.Sin(speed_vector) } * speed.Length;
                    corner_points.Add(new DataPoint(corner_points.Last().X + speed.X, corner_points.Last().Y + speed.Y));
                }

                List<double> corner_x = new List<double>();
                List<double> corner_y = new List<double>();
                foreach (DataPoint dp in corner_points)
                {
                    corner_x.Add(dp.X);
                    corner_y.Add(dp.Y);
                }
                var r_x = corner_x.Max() - corner_x.Min();
                var r_y = corner_y.Max() - corner_y.Min();

                {
                    if (corner_x.Average() > 0)
                    {
                        label1.Location = new System.Drawing.Point { X = 545, Y = 592 };
                    }
                    else
                    {
                        label1.Location = new System.Drawing.Point { X = 45, Y = 592 };
                    }
                }

                corner_line.Points.AddRange(corner_points);
                braking_line_model.Series.Add(corner_line);


                label1.Text = "V " + ((int)(veh_speed * 3.6)).ToString() + " kph" +
                "\n" + "R " + ((int)((r_x + r_y) / 4)).ToString() + " m" +
                "\n" + "A " + Math.Abs((int)(Math.Atan2(add_y, add_x) * 180 / Math.PI) - 90).ToString() + " deg";
            }
            
            catch 
            {
                label1.Text = "V " + ((int)(veh_speed * 3.6)).ToString() + " kph" +
                "\nR N/A m" +
                "\nA N/A deg";
            }

            braking_line_model.Series.Add(braking_line);
            braking_line_model.Series.Add(last_point);


            car_braking_line.Model = braking_line_model;

            List<double> data_x = new List<double>();
            List<double> data_y = new List<double>();
            foreach (DataPoint dp in brake_line)
            {
                data_x.Add(dp.X);
                data_y.Add(dp.Y);
            }


            

            //var x_min = car_braking_line.Model.Axes[0].ActualMinimum;
            //var x_max = car_braking_line.Model.Axes[0].ActualMinimum;
            //var x_center = (x_min + x_max) / 2;
            //var x_range = x_max - x_min;
            //var y_min = car_braking_line.Model.Axes[1].ActualMinimum;
            //var y_max = car_braking_line.Model.Axes[1].ActualMaximum;
            //var y_center = (y_min + y_max) / 2;
            //var y_range = y_max - y_min;

            //if (x_range > y_range)
            //{
            //    car_braking_line.Model.Axes[1].Zoom(y_center - x_range / 2, y_center + x_range / 2);
            //}
            //else
            //{
            //    car_braking_line.Model.Axes[0].Zoom(x_center - y_range / 2, x_center + y_range / 2);

            //}
            if ((data_x.Min() + data_x.Max()) > (data_y.Min() + data_y.Max()))
            {
                var center = (data_y.Min() + data_y.Max()) / 2;
                var center_x = (data_x.Min() + data_x.Max()) / 2;
                var range = data_x.Max() - data_x.Min();
                car_braking_line.Model.Axes[0].Zoom(center_x - range / 2 - 5, center_x + range / 2 - 5);
                car_braking_line.Model.Axes[1].Zoom(0, range + 10);
            }
            else
            {
                var center = (data_x.Min() + data_x.Max()) / 2;
                var center_y = (data_y.Min() + data_y.Max()) / 2;
                var range = data_y.Max() - data_y.Min();
                car_braking_line.Model.Axes[0].Zoom(center - range / 2 -5 , center + range / 2 + 5);
                car_braking_line.Model.Axes[1].Zoom(0 , range + 10);
            }
            //this.Text = r_x.ToString() + " | " + (r_x/2).ToString();

            
        }

        public void Zoom()
        {
            
        }


    }
}
