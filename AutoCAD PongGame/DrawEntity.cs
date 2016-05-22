using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;

namespace PongGame
{
    class DrawEntity
    {
        /// <summary>
        /// Z E Command
        /// </summary>
        public static void ZoomExtents()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("._z e ", true, false, false);
        }
        /// <summary>
        /// Create a new layer. Checks if the layer exist first.
        /// </summary>
        /// <param name="layerName">Name of the new layer</param>
        /// <param name="colour">Colour of the new layer</param>
        private static void CreateLayer(string layerName, short colour)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database currentDB = doc.Database;
            using (Transaction tr = currentDB.TransactionManager.StartTransaction())
            {
                LayerTable layerTable = (LayerTable)tr.GetObject(currentDB.LayerTableId, OpenMode.ForWrite);
                if (!layerTable.Has(layerName))
                {
                    LayerTableRecord layer = new LayerTableRecord();
                    layer.Name = layerName;
                    layer.Color = Color.FromColorIndex(ColorMethod.ByLayer, colour);
                    ObjectId layerID = layerTable.Add(layer);
                    tr.AddNewlyCreatedDBObject(layer, true);
                    tr.Commit();
                }
            }
        }
        /// <summary>
        /// Draw 4 point polygon
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="pt3"></param>
        /// <param name="pt4"></param>
        /// <param name="layerName"> Sets the layer of the polygon. Will create a new layer if layerName is not in the dwg</param>
        /// <param name="colour">Layer Colour</param>
        /// <param name="isClosed">Whether the polyline is closed or not</param>
        /// <returns></returns>
        public static Entity DrawBox(Point2d pt1, Point2d pt2, Point2d pt3, Point2d pt4, string layerName, short colour = 255, bool isClosed = true)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database currentDB = doc.Database;
            CreateLayer(layerName, colour);
            Entity entCreated;
            using (Transaction tr = currentDB.TransactionManager.StartTransaction())
            {
                BlockTable blckTable = tr.GetObject(currentDB.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord blockTableRec = tr.GetObject(blckTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                Polyline polyLine = new Polyline();
                polyLine.AddVertexAt(0, pt1, 0, 0, 0);
                polyLine.AddVertexAt(1, pt2, 0, 0, 0);
                polyLine.AddVertexAt(2, pt3, 0, 0, 0);
                polyLine.AddVertexAt(3, pt4, 0, 0, 0);
                polyLine.Closed = isClosed;
                polyLine.Layer = layerName;
                blockTableRec.AppendEntity(polyLine);
                tr.AddNewlyCreatedDBObject(polyLine, true);
                tr.Commit();
                entCreated = polyLine;
            }
            return entCreated;
        }

        /// <summary>
        /// Creates an MText
        /// </summary>
        /// <param name="pt1">Point of origin</param>
        /// <param name="textVal">Contents of the Mtxt</param>
        /// <param name="layerName"></param>
        /// <param name="colour"></param>
        /// <returns></returns>
        public static Entity DrawText(Point2d pt1, string textVal,  string layerName, short colour = 255)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database currentDB = doc.Database;
            CreateLayer(layerName, colour);
            Entity entCreated;
            using (Transaction tr = currentDB.TransactionManager.StartTransaction())
            {
                BlockTable blckTable = tr.GetObject(currentDB.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord blockTableRec = tr.GetObject(blckTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                MText text = new MText();
                text.Contents = textVal;
                text.TextHeight = 1;
                text.Location = new Point3d(pt1.X, pt1.Y, 0);
                text.Layer = layerName;
                blockTableRec.AppendEntity(text);
                tr.AddNewlyCreatedDBObject(text, true);
                tr.Commit();
                entCreated = text;
            }
            return entCreated;
        }
        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <param name="ballRadius"></param>
        /// <param name="ballCentre"></param>
        /// <param name="layerName"></param>
        /// <param name="colour"></param>
        /// <returns></returns>
        public static Entity DrawCircle(double ballRadius, Point3d ballCentre, string layerName, short colour = 255)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database currentDB = doc.Database;
            CreateLayer(layerName, colour);
            Entity entCreated;
            using (Transaction tr = currentDB.TransactionManager.StartTransaction())
            {
                BlockTable blckTable = tr.GetObject(currentDB.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord blockTableRec = tr.GetObject(blckTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                Circle ball = new Circle();
                ball.Radius = ballRadius;
                ball.Center = ballCentre;
                ball.Layer = layerName;
                blockTableRec.AppendEntity(ball);
                tr.AddNewlyCreatedDBObject(ball, true);
                tr.Commit();
                entCreated = ball;
            }
            return entCreated;
        }
    }
}
