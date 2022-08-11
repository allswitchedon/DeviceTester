using OxyPlot;
using OxyPlot.Annotations;
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
using System.Windows.Forms;

namespace WinFormsApp2
{
    public partial class Form4 : Form
    {

        public Form4(Collection<DataPoint> steering, Collection<DataPoint> braking)
        {
            InitializeComponent();

            var steering_and_brake_model = new PlotModel();
            var steering_line = new LineSeries();
            var braking_line = new LineSeries();
            braking_line.Color = OxyColors.OrangeRed;
            steering_line.Color = OxyColors.Green;
            var x_axis = new LinearAxis();
            var steering_axis = new LinearAxis();
            var braking_axis = new LinearAxis();
            x_axis.Position = AxisPosition.Bottom;
            x_axis.Key = "time";
            steering_axis.Position = AxisPosition.Right;
            steering_axis.Key = "steering";
            braking_axis.Position = AxisPosition.Left;
            braking_axis.Key = "braking";
            steering_line.Points.AddRange(steering);
            steering_line.YAxisKey = "steering";
            steering_line.XAxisKey = "time";
            braking_line.Points.AddRange(braking);
            braking_line.YAxisKey = "braking";
            braking_line.XAxisKey = "time";
            steering_and_brake_model.Axes.Add(x_axis);
            steering_and_brake_model.Axes.Add(steering_axis);
            steering_and_brake_model.Axes.Add(braking_axis);
            steering_and_brake_model.Series.Add(steering_line);
            steering_and_brake_model.Series.Add(braking_line);

            plotView1.Model = steering_and_brake_model;
            //plotView1.Model.Axes[1].Zoom(0, 1);
            plotView1.Model.Axes[2].Zoom(0, 1);

        }

        private void Form4_Load(object sender, EventArgs e)
        {

        }

        public void Update(Collection<DataPoint> steering, Collection<DataPoint> braking)
        {

            var steering_and_brake_model = new PlotModel();
            var steering_line = new LineSeries();
            var braking_line = new LineSeries();
            var max_brake = new LineAnnotation { Type = LineAnnotationType.Horizontal};
            max_brake.Color = OxyColors.Black;
            max_brake.YAxisKey = "braking";
            max_brake.LineStyle = LineStyle.Solid;
            max_brake.StrokeThickness = 2;
            max_brake.Y = 0.85;
            braking_line.Color = OxyColors.OrangeRed;
            steering_line.Color = OxyColors.Green;
            var x_axis = new LinearAxis();
            var steering_axis = new LinearAxis();
            var braking_axis = new LinearAxis();
            x_axis.Position = AxisPosition.Bottom;
            x_axis.Key = "time";
            steering_axis.Position = AxisPosition.Right;
            steering_axis.Key = "steering";
            braking_axis.Position = AxisPosition.Left;
            braking_axis.Key = "braking";
            steering_line.Points.AddRange(steering);
            steering_line.YAxisKey = "steering";
            steering_line.XAxisKey = "time";
            braking_line.Points.AddRange(braking);
            braking_line.YAxisKey = "braking";
            braking_line.XAxisKey = "time";
            braking_line.StrokeThickness = 3;
            x_axis.MinorGridlineStyle = LineStyle.Solid;
            x_axis.MajorGridlineStyle = LineStyle.Solid;
            braking_axis.MinorGridlineStyle = LineStyle.Solid;
            braking_axis.MajorGridlineStyle = LineStyle.Solid;
            steering_and_brake_model.Axes.Add(x_axis);
            steering_and_brake_model.Axes.Add(steering_axis);
            steering_and_brake_model.Axes.Add(braking_axis);
            steering_and_brake_model.Series.Add(braking_line);
            steering_and_brake_model.Series.Add(steering_line);
            steering_and_brake_model.Annotations.Add(max_brake);

            plotView1.Model = steering_and_brake_model;

        }

        private void plotView1_DoubleClick(object sender, EventArgs e)
        {
            plotView1.Model.ResetAllAxes();
            plotView1.OnModelChanged();
        }
    }
}
