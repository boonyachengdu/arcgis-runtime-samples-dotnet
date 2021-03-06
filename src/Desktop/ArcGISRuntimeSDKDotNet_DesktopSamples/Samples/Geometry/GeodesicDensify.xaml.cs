﻿using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates using GeometryEngine.GeodesicDensify to take an input shape and return a geodesic densified shape. Original vertices to create the original polygon or polyline are shown in red. The returned polygon shows the additional densified vertices in green. To use the sample, click the 'Densify' button and create a shape on the map. Double-click to end the polygon sketch and densify the shape and see the original and densified vertices.
    /// </summary>
    /// <title>Geodesic Densify</title>
	/// <category>Geometry</category>
	public partial class GeodesicDensify : UserControl
    {
        private const double METERS_TO_MILES = 0.000621371192;
        private const double SQUARE_METERS_TO_MILES = 3.86102159e-7;

        private Symbol _lineSymbol;
        private Symbol _fillSymbol;
        private Symbol _origVertexSymbol;
        private Symbol _newVertexSymbol;

        /// <summary>Construct Densify sample control</summary>
        public GeodesicDensify()
        {
            InitializeComponent();

            _lineSymbol = layoutGrid.Resources["LineSymbol"] as Symbol;
            _fillSymbol = layoutGrid.Resources["FillSymbol"] as Symbol;
            _origVertexSymbol = layoutGrid.Resources["OrigVertexSymbol"] as Symbol;
            _newVertexSymbol = layoutGrid.Resources["NewVertexSymbol"] as Symbol;
        }

        // Draw and densify a user defined polygon
        private async void DensifyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                resultsPanel.Visibility = Visibility.Collapsed;
                inputGraphics.Graphics.Clear();
                resultGraphics.Graphics.Clear();

                // Request polygon or polyline from the user
                DrawShape drawShape = (DrawShape)comboShapeType.SelectedItem;
                var original = await mapView.Editor.RequestShapeAsync(drawShape, _fillSymbol);

                // Add original shape vertices to input graphics layer
                var coordsOriginal = ((IEnumerable<CoordinateCollection>)original).First();
                foreach (var coord in coordsOriginal)
                    inputGraphics.Graphics.Add(new Graphic(new MapPoint(coord, original.SpatialReference), _origVertexSymbol));

                // Densify the shape
                var densify = GeometryEngine.GeodesicDensify(original, mapView.Extent.Width / 100, LinearUnits.Meters);
                inputGraphics.Graphics.Add(new Graphic(densify, _fillSymbol));

                // Add new vertices to result graphics layer
                var coordsDensify = ((IEnumerable<CoordinateCollection>)densify).First();
                foreach (var coord in coordsDensify)
                    resultGraphics.Graphics.Add(new Graphic(new MapPoint(coord, original.SpatialReference), _newVertexSymbol));

                // Results
                Dictionary<string, object> results = new Dictionary<string, object>();
                results["Length"] = GeometryEngine.GeodesicLength(densify) * METERS_TO_MILES;
                if (original is Polygon)
                    results["Area"] = GeometryEngine.GeodesicArea(densify) * SQUARE_METERS_TO_MILES;
                else
                    results["Area"] = "N/A";
                results["Vertices Before"] = coordsOriginal.Count();
                results["Vertices After"] = coordsDensify.Count();

                resultsListView.ItemsSource = results;
                resultsPanel.Visibility = Visibility.Visible;
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Densify Error: " + ex.Message, "Geodesic Densify Sample");
            }
        }
    }
}
