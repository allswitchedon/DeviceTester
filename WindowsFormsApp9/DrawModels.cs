using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Legends;
using OxyPlot.Series;
using System.Collections.Generic;

namespace WinFormsApp2
{
    internal class DrawModels
    {

        public static PlotModel Latency(List<float> time, List<List<float>> input, List<List<float>> outpunt, int input_id, int output_id)
        {
            var model = new PlotModel();
            var legend = new Legend();
            var input_line = new LineSeries { Title = "Device" };
            var output_line = new LineSeries { Title = "Game" };
            for (int i = 0; i < time.Count; i++)
            {
                input_line.Points.Add(new DataPoint(time[i], input[i][input_id]));
                output_line.Points.Add(new DataPoint(time[i], outpunt[i][output_id]));
            }
                model.Annotations.Add(new LineAnnotation { Type = LineAnnotationType.Horizontal, Y = 0, Color = OxyColors.Black, LineStyle = LineStyle.Solid}) ;
            model.Legends.Add(legend);
            model.Series.Add(input_line);
            model.Series.Add(output_line);
            return model;
        }
        public static PlotModel Latency(List<float> time, List<List<float>> input, List<List<float>> outpunt, int input_id, int input_id2, int output_id)
        {
            var model = new PlotModel();
            var legend = new Legend();
            var input_line = new LineSeries { Title = "Device" };
            var output_line = new LineSeries { Title = "Game" };
            for (int i = 0; i < time.Count; i++)
            {
                input_line.Points.Add(new DataPoint(time[i], (input[i][input_id] + input[i][input_id2]) * outpunt[i][output_id]));
                //if (input[i][input_id] == 1)`
                //{
                //    model.Annotations.Add(new LineAnnotation { Type = LineAnnotationType.Vertical, MinimumY = 0, MaximumY = 6, Color = OxyColors.Red, X = time[i] , LineStyle = LineStyle.Solid}) ;
                //}
                //if (input[i][input_id2] == 1)
                //{
                //    model.Annotations.Add(new LineAnnotation { Type = LineAnnotationType.Vertical, MinimumY = 0, MaximumY = 6, Color = OxyColors.Blue, X = time[i], LineStyle = LineStyle.Solid });
                //}
                output_line.Points.Add(new DataPoint(time[i], outpunt[i][output_id]));
            }
            model.Legends.Add(legend);
            model.Series.Add(input_line);
            model.Series.Add(output_line);
            return model;
        }

        public static LineSeries CircleLine(double r, OxyColor color)
        {
            var circle = new LineSeries();
            circle.InterpolationAlgorithm = InterpolationAlgorithms.ChordalCatmullRomSpline;
            circle.Color = color;
            var rm = r;
            r = r * -1;
            circle.Points.Add(new DataPoint(0, -rm));
            while (r <= rm)
            {
                circle.Points.Add(new DataPoint(System.Math.Sqrt(rm*rm - (r * r)), r));
                r = r + 0.01;
            }
            circle.Points.Add(new DataPoint(0, rm));
            while (r > -rm)
            {
                r = r - 0.01;
                circle.Points.Add(new DataPoint(-System.Math.Sqrt(rm * rm - (r * r)), r));

            }
            circle.Points.Add(new DataPoint(0, -rm));
            return circle;
        }

        public static LineSeries LeftFootBraking_Line(double r, OxyColor color)
        {
            var circle = new LineSeries();
            circle.InterpolationAlgorithm = InterpolationAlgorithms.ChordalCatmullRomSpline;
            circle.Color = color;
            var rm = r;
            r = r * -1;
            circle.Points.Add(new DataPoint(0, -rm));
            while (r <= rm)
            {
                circle.Points.Add(new DataPoint(System.Math.Sqrt(rm * rm - (r * r)), r));
                r = r + 0.01;
            }
            circle.Points.Add(new DataPoint(0, rm));
            while (r > -rm)
            {
                r = r - 0.01;
                circle.Points.Add(new DataPoint(-System.Math.Sqrt(rm * rm - (r * r)), r));

            }
            circle.Points.Add(new DataPoint(0, -rm));
            return circle;
        }
    }
}
