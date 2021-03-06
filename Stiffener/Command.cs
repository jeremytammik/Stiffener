﻿#region Namespaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace Stiffener
{
  [Transaction( TransactionMode.Manual )]
  public class Command : IExternalCommand
  {
    #region Constants
    /// <summary>
    /// Family template filename extension
    /// </summary>
    const string _family_template_ext = ".rft";

    /// <summary>
    /// Revit family filename extension
    /// </summary>
    const string _rfa_ext = ".rfa";

    /// <summary>
    /// Family template library path
    /// </summary>
    //const string _path = "C:/ProgramData/Autodesk/RST 2012/Family Templates/English";
    const string _path = "C:/Users/All Users/Autodesk/RVT 2014/Family Templates/English";

    /// <summary>
    /// Family template filename stem
    /// </summary>
    const string _family_template_name = "Metric Structural Stiffener";

    // Family template path and filename for imperial units

    //const string _path = "C:/ProgramData/Autodesk/RST 2012/Family Templates/English_I";
    //const string _family_name = "Structural Stiffener";

    /// <summary>
    /// Name of the generated stiffener family
    /// </summary>
    const string _family_name = "Stiffener";

    /// <summary>
    /// Conversion factor from millimetre to foot
    /// </summary>
    const double _mm_to_foot = 1 / 304.8;
    #endregion // Constants

    /// <summary>
    /// Convert a given length to feet
    /// </summary>
    double MmToFoot( double length_in_mm )
    {
      return _mm_to_foot * length_in_mm;
    }

    /// <summary>
    /// Convert a given point defined in millimetre units to feet
    /// </summary>
    XYZ MmToFootPoint( XYZ p )
    {
      return p.Multiply( _mm_to_foot );
    }

    static int n = 4;

    /// <summary>
    /// Extrusion profile points defined in millimetres.
    /// Here is just a very trivial rectangular shape.
    /// </summary>
    static List<XYZ> _countour = new List<XYZ>( n )
    {
      new XYZ( 0 , -75 , 0 ),
      new XYZ( 508, -75 , 0 ), 
      new XYZ( 508, 75 , 0 ),
      new XYZ( 0, 75 , 0 )
    };

    /// <summary>
    /// Extrusion thickness for stiffener plate
    /// </summary>
    const double _thicknessMm = 20.0;

    /// <summary>
    /// Return the first element found of the 
    /// specific target type with the given name.
    /// </summary>
    Element FindElement(
      Document doc,
      Type targetType,
      string targetName )
    {
      return new FilteredElementCollector( doc )
        .OfClass( targetType )
        .First<Element>( e => e.Name.Equals( targetName ) );

      // Obsolete code parsing the collection for the 
      // given name using a LINQ query. 

      //var targetElems 
      //  = from element in collector 
      //    where element.Name.Equals( targetName ) 
      //    select element;

      //return targetElems.First<El
      //List<Element> elems = targetElems.ToList<Element>();

      //if( elems.Count > 0 )
      //{  // we should have only one with the given name. 
      //  return elems[0];
      //}

      // cannot find it.
      //return null;

      /*
      // most efficient way to find a named 
      // family symbol: use a parameter filter.

      ParameterValueProvider provider
        = new ParameterValueProvider(
          new ElementId( BuiltInParameter.DATUM_TEXT ) ); // VIEW_NAME for a view
 
      FilterStringRuleEvaluator evaluator
        = new FilterStringEquals();
 
      FilterRule rule = new FilterStringRule(
        provider, evaluator, targetName, true );
 
      ElementParameterFilter filter
        = new ElementParameterFilter( rule );

      return new FilteredElementCollector( doc )
        .OfClass( targetType )
        .WherePasses( filter )
        .FirstElement();
      */
    }

    /// <summary>
    /// Convert a given list of XYZ points 
    /// to a CurveArray instance. 
    /// The points are defined in millimetres, 
    /// the returned CurveArray in feet.
    /// </summary>
    CurveArray CreateProfile( List<XYZ> pts )
    {
      CurveArray profile = new CurveArray();

      int n = _countour.Count;

      for( int i = 0; i < n; ++i )
      {
        int j = ( 0 == i ) ? n - 1 : i - 1;

        profile.Append( Line.CreateBound(
          MmToFootPoint( pts[j] ),
          MmToFootPoint( pts[i] ) ) );
      }
      return profile;
    }

    /// <summary>
    /// Create an extrusion from a given thickness 
    /// and list of XYZ points defined in millimetres
    /// in the given family document, which  must 
    /// contain a sketch plane named "Ref. Level".
    /// </summary>
    Extrusion CreateExtrusion(
      Document doc,
      List<XYZ> pts,
      double thickness )
    {
      Autodesk.Revit.Creation.FamilyItemFactory factory
        = doc.FamilyCreate;

      SketchPlane sketch = FindElement( doc,
        typeof( SketchPlane ), "Ref. Level" )
          as SketchPlane;

      CurveArrArray curveArrArray = new CurveArrArray();

      curveArrArray.Append( CreateProfile( pts ) );

      double extrusionHeight = MmToFoot( thickness );

      return factory.NewExtrusion( true,
        curveArrArray, sketch, extrusionHeight );
    }

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Application app = uiapp.Application;
      Document doc = uidoc.Document;
      Document fdoc = null;
      Transaction t = null;

      if( null == doc )
      {
        message = "Please run this command in an open document.";
        return Result.Failed;
      }

      #region Create a new structural stiffener family

      // Check whether the family has already
      // been created or loaded previously.

      Family family
        = new FilteredElementCollector( doc )
          .OfClass( typeof( Family ) )
          .Cast<Family>()
          .FirstOrDefault<Family>( e
            => e.Name.Equals( _family_name ) );

      if( null != family )
      {
        fdoc = family.Document;
      }
      else
      {
        string templateFileName = Path.Combine( _path,
          _family_template_name + _family_template_ext );

        fdoc = app.NewFamilyDocument(
          templateFileName );

        if( null == fdoc )
        {
          message = "Cannot create family document.";
          return Result.Failed;
        }

        using( t = new Transaction( fdoc ) )
        {
          t.Start( "Create structural stiffener family" );

          CreateExtrusion( fdoc, _countour, _thicknessMm );

          t.Commit();
        }

        //fdoc.Title = _family_name; // read-only property

        bool needToSaveBeforeLoad = false;

        if( needToSaveBeforeLoad )
        {
          // Save our new family background document
          // and reopen it in the Revit user interface.

          string filename = Path.Combine(
            Path.GetTempPath(), _family_name + _rfa_ext );

          SaveAsOptions opt = new SaveAsOptions();
          opt.OverwriteExistingFile = true;

          fdoc.SaveAs( filename, opt );

          bool closeAndOpen = true;

          if( closeAndOpen )
          {
            // Cannot close the newly generated family file
            // if it is the only open document; that throws 
            // an exception saying "The active document may 
            // not be closed from the API."

            fdoc.Close( false );

            // This obviously invalidates the uidoc 
            // instance on the previously open document.
            //uiapp.OpenAndActivateDocument( filename );
          }
        }
      }
      #endregion // Create a new structural stiffener family

      #region Load the structural stiffener family

      // Must be outside transaction; otherwise Revit 
      // throws InvalidOperationException: The document 
      // must not be modifiable before calling LoadFamily. 
      // Any open transaction must be closed prior the call.

      // Calling this without a prior call to SaveAs
      // caused a "serious error" in Revit 2012:

      family = fdoc.LoadFamily( doc );

      // Workaround for Revit 2012, 
      // no longer needed in Revit 2014:

      //doc.LoadFamily( filename, out family );

      // Setting the name requires an open 
      // transaction, of course.

      //family.Name = _family_name;

      FamilySymbol symbol = null;

      foreach( ElementId id 
        in family.GetFamilySymbolIds() )
      {
        // Our family only contains one
        // symbol, so pick it and leave.

        symbol = doc.GetElement( id ) as FamilySymbol;

        break;
      }
      #endregion // Load the structural stiffener family

      #region Insert stiffener family instance

      using( t = new Transaction( doc ) )
      {
        t.Start( "Insert structural stiffener family instance" );

        // Setting the name requires an open 
        // transaction, of course.

        family.Name = _family_name;
        symbol.Name = _family_name;

        // Need to activate symbol before 
        // using it in Revit 2016.

        symbol.Activate();

        bool useSimpleInsertionPoint = true;

        if( useSimpleInsertionPoint )
        {
          //Plane plane = app.Create.NewPlane( new XYZ( 1, 2, 3 ), XYZ.Zero );
          //SketchPlane sketch = doc.Create.NewSketchPlane( plane );
          //commandData.View.SketchPlane = sketch;

          XYZ p = uidoc.Selection.PickPoint(
            "Please pick a point for family instance insertion" );

          StructuralType st = StructuralType.UnknownFraming;

          doc.Create.NewFamilyInstance( p, symbol, st );
        }

        bool useFaceReference = false;

        if( useFaceReference )
        {
          Reference r = uidoc.Selection.PickObject(
            ObjectType.Face,
            "Please pick a point on a face for family instance insertion" );

          Element e = doc.GetElement( r.ElementId );
          GeometryObject obj = e.GetGeometryObjectFromReference( r );
          PlanarFace face = obj as PlanarFace;

          if( null == face )
          {
            message = "Please select a point on a planar face.";
            t.RollBack();
            return Result.Failed;
          }
          else
          {
            XYZ p = r.GlobalPoint;
            //XYZ v = face.Normal.CrossProduct( XYZ.BasisZ ); // 2015
            XYZ v = face.FaceNormal.CrossProduct( XYZ.BasisZ ); // 2016
            if( v.IsZeroLength() )
            {
              v = face.FaceNormal.CrossProduct( XYZ.BasisX );
            }
            doc.Create.NewFamilyInstance( r, p, v, symbol );

            // This throws an exception saying that the face has no reference on it:
            //doc.Create.NewFamilyInstance( face, p, v, symbol ); 
          }
        }
        t.Commit();
      }
      #endregion // Insert stiffener family instance

      return Result.Succeeded;
    }
  }
}
