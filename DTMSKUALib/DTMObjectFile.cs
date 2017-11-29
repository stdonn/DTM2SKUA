//
//  EmptyClass.cs
//
//  Author:
//       Stephan Donndorf <stephan.donndorf@googlemail.com>
//
//  Copyright (c) 2016 Stephan Donndorf
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DTMSKUALib
{
	public class DTMObjectFile
	{
		public DTMObjectFile()
		{
			_DTMList = new List<DTMObject>();
			_StringDict = new Dictionary<int, StringObject>();
		}

		private List<DTMObject> _DTMList;
		private Dictionary<int, StringObject> _StringDict;

		public Dictionary<int, StringObject> StringDict {
			get {
				return _StringDict;
			}
		}

		public void addObject( DTMObject obj )
		{
			_DTMList.Add( obj );
		}

		public DTMObject removeObject()
		{
			DTMObject elem = _DTMList[_DTMList.Count - 1];
			if ( !_DTMList.Remove( elem ) )
				Console.WriteLine( "Cannot remove DTMObject from List!" );
			return elem;
		}

		protected void onFileNotFound( string path )
		{
			FileNotFoundException ex = new FileNotFoundException( "Specified file doesn't exist!" );
			ex.Data.Add( "Time", DateTime.Now );
			ex.Data.Add( "File", path );

			throw ex;
		}

		public bool readDTMFile( string path )
		{
			if ( !File.Exists( path ) ) {
				onFileNotFound( path );
				return false;
			}

			Stopwatch watch = new Stopwatch();
			watch.Start();
			Console.WriteLine( "Start reading DTM file..." );
			string STRFileName = Path.GetDirectoryName(path);
			int lineNr = 0;
			DTMObject dtm = new DTMObject();
			dtm.objects = new Dictionary<int, int[]>();

			string[] lines = File.ReadAllLines( path );
			foreach ( string line in lines ) {

				lineNr++;
				// ignore second and third line
				if ( lineNr == 2 )
					continue;

				string[] parts = line.Split( ',' );

				if ( lineNr == 3 ) {
					dtm.objectNumber = Convert.ToInt32( parts[1] );
					continue;
				}

				// first line in file -> read String filename!
				if ( lineNr == 1 ) {
					STRFileName = Path.Combine(STRFileName, line.Split( ',' )[0]);

					// string-file is not exisiting? -> Exception
					if ( !File.Exists( STRFileName ) ) {
						onFileNotFound( STRFileName );
						return false;
					}
					continue;
				}

				if ( parts.Length > 0 ) {
					switch ( parts[0] ) {
						case "TRISOLATION":  // New trisolation information
							dtm.configLine = line.TrimEnd();
							// if more than one trisolation per object -> insert and create a new object
							if ( dtm.objects.Count > 0 ) {
								addObject( dtm );
								DTMObject dtm2 = new DTMObject();
								dtm2.objects = new Dictionary<int, int[]>();
								dtm2.objectNumber = dtm.objectNumber;
								dtm = dtm2;
							}
							break;
						case "END":
						case "OBJECT":  // save object to List, first occurance in line 3 is skipped before!
							addObject( dtm );
							if ( parts[0] == "OBJECT" ) {
								dtm = new DTMObject();
								dtm.objects = new Dictionary<int, int[]>();
								dtm.objectNumber = Convert.ToInt32( parts[1] );
							}
							break;
						default:  // add triangle to the list
							int[] numbers;

							// dtm with neighbouring information ? numbers[6] : numbers[3]
							try {
								if ( parts.Length > 6 )
									numbers = new int[] {
										Convert.ToInt32( parts[1] ), Convert.ToInt32( parts[2] ), Convert.ToInt32( parts[3] ), Convert.ToInt32( parts[4] ), Convert.ToInt32( parts[5] ), Convert.ToInt32( parts[6] ) };
								else
									numbers = new int[] { Convert.ToInt32( parts[1] ), Convert.ToInt32( parts[2] ), Convert.ToInt32( parts[3] ) };
							}

							catch ( Exception ex ) {
								Console.WriteLine( "An exception occured!" );
								Console.WriteLine( ex.Message );
								foreach ( string key in ex.Data.Keys )
									Console.WriteLine( key + " - " + ex.Data[key] );
								Console.WriteLine( "Conversation line: \"" + line + "\"" );
								return false;
							}

							// Console.WriteLine( "Numbers: " + numbers?.Length.ToString() );
							// Console.WriteLine( "Parts: " + parts?.Length.ToString() );

							try { dtm.objects.Add( Convert.ToInt32( parts[0] ), numbers ); }
							catch ( Exception ex ) {
								Console.WriteLine( "An exception occured!" );
								Console.WriteLine( ex.Message );
								foreach ( string key in ex.Data.Keys )
									Console.WriteLine( key + " - " + ex.Data[key] );
								Console.WriteLine( "Conversation line: \"" + line + "\"" );
								return false;
							}
							break;
					}
				}
			}

			watch.Stop();
			Console.WriteLine( "Finished readind DTM file.\nFound {0} new Objects in the DTM file!\n", _DTMList.Count );
			Console.WriteLine( "Time: {0} ms", watch.ElapsedMilliseconds );
			Console.WriteLine( "Start reading string file..." );

			watch.Restart();

			lineNr = 0;
			lines = File.ReadAllLines( STRFileName );
			foreach ( string line in lines ) {
				lineNr++;

				// ignore first 2 lines
				if ( lineNr < 3 )
					continue;

				string[] parts = line.Split( ',' );
				if ( parts.Length < 4 ) {
					Console.WriteLine( "To Less parts! Ignoring..." );
					Console.WriteLine( "Line: " + line );
					continue;
				}
				if ( parts[0] == "0" )
					continue;

				try {
					if ( _StringDict.ContainsKey( lineNr - 2 ) ) {
						KeyExistingException ex = new KeyExistingException( "Key " + ( lineNr + 2 ) + " is existing in dictionary!" );
						ex.Data.Add( "Time", DateTime.Now );
						ex.Data.Add( "Key", lineNr + 2 );
						ex.Data.Add( "Existing Object", _StringDict[lineNr + 2] );
						ex.Data.Add( "Current Line", line );
						throw ex;
					}

					StringObject strObj = new StringObject();
					// ATTENTION: SURPAC first stores Y-Coordinate, after that X-Coordinate
					strObj.Y = ( float ) Convert.ToDouble( parts[1] );
					strObj.X = ( float ) Convert.ToDouble( parts[2] );
					strObj.Z = ( float ) Convert.ToDouble( parts[3] );
					strObj.ID = Convert.ToInt32( parts[0] );

					_StringDict[lineNr - 2] = strObj;
				}
				catch ( KeyExistingException ex ) {
					Console.WriteLine( ex.Message );
					foreach ( string key in ex.Data.Keys )
						Console.WriteLine( key + " - " + ex.Data[key] );
					Console.WriteLine( "Skipping this line...\n" );
					continue;
				}
				catch ( Exception ex ) {
					Console.WriteLine( "An exception occured!" );
					Console.WriteLine( ex.Message );
					foreach ( string key in ex.Data.Keys )
						Console.WriteLine( key + " - " + ex.Data[key] );
					Console.WriteLine( "Conversation line: \"" + line + "\"" );
					return false;
				}
			}

			// foreach ( int index in _StringDict.Keys ) {
			// 	Console.WriteLine( _StringDict[index].ID + " - " + _StringDict[index].X + " - " + _StringDict[index].Y + " - " + _StringDict[index].Z );
			// }

			watch.Stop();
			Console.WriteLine( "Finished reading string file.\nFound {0} vertices in the string file!\n", _StringDict.Count );
			Console.WriteLine( "Time: {0} ms", watch.ElapsedMilliseconds );

			return true;
		}

		public bool readSKUAFile( string path )
		{
			if ( !File.Exists( path ) ) {
				onFileNotFound( path );
				return false;
			}

			Console.WriteLine( "Start reading SKUA-GOCAD surface file..." );
			DTMObject dtm = new DTMObject();
			dtm.objects = new Dictionary<int, int[]>();
			int OIDField = 0, properties = 0, lastVertexID = 0, existingVerts = 0, triangleNr = 0;
			bool first = true;
			List<int> idList = new List<int>();

			string[] lines = File.ReadAllLines( path );
			foreach ( string line in lines ) {
				//Console.Write( line + " ... " );
				// line == "END" ? save and new DTM; reset also OIDField : continue;
				string trimed = line.TrimEnd();
				if ( trimed == "END" ) {
					//Console.WriteLine( "END" );
					if ( OIDField != 0 ) {
						int most = idList.GroupBy( i => i ).OrderByDescending( grp => grp.Count() ).Select( grp => grp.Key ).First();
						dtm.objectNumber = most;
						//Console.WriteLine( "Setting objectNr to {0}", most );
					} else {
						dtm.objectNumber = 32000;
						//Console.WriteLine( "OID not available. Setting objectNr to 32000" );
					}
					addObject( dtm );
					dtm = new DTMObject();
					dtm.objects = new Dictionary<int, int[]>();
					OIDField = 0;
					properties = 0;
					first = true;
					existingVerts += lastVertexID;
					lastVertexID = 0;
					idList.Clear();
					continue;
				}

				// line == "TFACE" ? save and new DTM; DON'T reset also OIDField : continue;
				// skip for first TFACE
				if ( trimed == "TFACE" ) {
					//Console.WriteLine( "TFACE" );
					if ( first ) {
						//Console.WriteLine( "... First occurance..." );
						first = false;
						continue;
					}
					if ( OIDField != 0 ) {
						int most = idList.GroupBy( i => i ).OrderByDescending( grp => grp.Count() ).Select( grp => grp.Key ).First();
						dtm.objectNumber = most;
						//Console.WriteLine( "Setting objectNr to {0}", most );
					} else {
						dtm.objectNumber = 32000;
						//Console.WriteLine( "OID not available. Setting objectNr to 32000" );
					}
					addObject( dtm );
					dtm = new DTMObject();
					dtm.objects = new Dictionary<int, int[]>();
					idList.Clear();
					continue;
				}

				string[] parts = line.Split( ' ' );
				if ( parts.Length < 2 ) {
					//Console.WriteLine( "< 2" );
					continue;
				}

				bool failure = false;
				StringObject strObj;
				try {
					switch ( parts[0] ) {
						case ( "PROPERTIES" ):
							//Console.WriteLine( "PROPERTIES" );
							properties++;
							if ( parts[1] == "OID" )
								OIDField = properties;
							break;
						case ( "VRTX" ):
							//Console.WriteLine( "VRTX" );
							if ( parts.Length < 5 ) {
								failure = true;
								continue;
							}
							strObj = new StringObject();
							strObj.X = ( float ) Convert.ToDouble( parts[2] );
							strObj.Y = ( float ) Convert.ToDouble( parts[3] );
							strObj.Z = ( float ) Convert.ToDouble( parts[4] );
							lastVertexID = Convert.ToInt32( parts[1] ); ;
							strObj.ID = 32000;
							idList.Add( strObj.ID );
							_StringDict.Add( lastVertexID + existingVerts, strObj );
							break;

						case ( "PVRTX" ):
							//Console.WriteLine( "PVRTX" );
							if ( parts.Length < ( 5 + OIDField ) ) {
								failure = true;
								break;
							}
							strObj = new StringObject();
							strObj.X = ( float ) Convert.ToDouble( parts[2] );
							strObj.Y = ( float ) Convert.ToDouble( parts[3] );
							strObj.Z = ( float ) Convert.ToDouble( parts[4] );
							lastVertexID = Convert.ToInt32( parts[1] );
							strObj.ID = ( OIDField != 0 ) ? Convert.ToInt32( parts[4 + OIDField] ) : 32000;
							idList.Add( strObj.ID );
							_StringDict.Add( lastVertexID + existingVerts, strObj );
							break;
						case ( "TRGL" ):
							//Console.WriteLine( "TRGL" );
							if ( parts.Length < 4 ) {
								failure = true;
								break;
							}
							triangleNr++;
							int[] points = { Convert.ToInt32( parts[1] ) + existingVerts, Convert.ToInt32( parts[2] ) + existingVerts, Convert.ToInt32( parts[3] ) + existingVerts };
							dtm.objects.Add( triangleNr, points );
							break;
						default:
							//Console.WriteLine( "default" );
							break;
					}
				}
				catch ( IndexOutOfRangeException ex ) {
					Console.WriteLine( "Index out of range Exception occured!" );
					Console.WriteLine( ex.Message );
					foreach ( string key in ex.Data.Keys )
						Console.WriteLine( key + " - " + ex.Data[key] );
					Console.WriteLine( "Conversation line: \"" + line + "\"" );
					return false;
				}
				catch ( Exception ex ) {
					Console.WriteLine( "An exception occured!" );
					Console.WriteLine( ex.Message );
					foreach ( string key in ex.Data.Keys )
						Console.WriteLine( key + " - " + ex.Data[key] );
					Console.WriteLine( "Conversation line: \"" + line + "\"" );
					return false;
				}
				if ( failure ) {
					Console.WriteLine( "WARNING: Cannot read line:" );
					Console.WriteLine( line );
					continue;
				}
			}
			return true;
		}

		public bool writeDTMFiles( string path )
		{
			Console.WriteLine( "\nStart writing string file..." );
			if ( File.Exists( path ) )
				Console.WriteLine( "WARNING: SKUA-GOCAD file {0} exists! Overwriting!", path );

			Stopwatch watch = new Stopwatch();
			watch.Start();

			// first: fileheader
			DateTime now = DateTime.Now;
			string startText = Path.GetFileNameWithoutExtension( path ) + "," + DateTime.Now.ToString( "dd-MMM-yy" ) + ",,\n";
			string separatorLine = "0, 0.000, 0.000, 0.000,\n";

			StringBuilder text = new StringBuilder( 4096 );
			text.Append( startText + separatorLine.Replace( " 0.000,", " 0.000, 0.000," ) );
			Dictionary<int, int> idDict = new Dictionary<int, int>();

			try {
				var sortedDict = _StringDict.OrderBy( idx => idx.Key ).GroupBy( i => i.Value.ID ).OrderBy( i => i.First().Value.ID );

				int lastID = sortedDict.First().First().Value.ID;
				int lineNr = 0;

				foreach ( var obj in sortedDict ) {
					foreach ( var lowerObj in obj ) {
						++lineNr;
						if ( lastID != lowerObj.Value.ID ) {
							text.Append( separatorLine );
							lastID = lowerObj.Value.ID;
							++lineNr;
						}
						idDict[lowerObj.Key] = lineNr;
						text.Append( lowerObj.Value.ID + ", " + lowerObj.Value.Y + ", " + lowerObj.Value.X + ", " + lowerObj.Value.Z + ", \n" );
					}
				}
			}
			catch ( Exception ex ) {
				Console.WriteLine( "An exception occured during var reading!" );
				Console.WriteLine( ex.Message );
				foreach ( string key in ex.Data.Keys )
					Console.WriteLine( key + " - " + ex.Data[key] );
				//Console.WriteLine( "Conversation line: \"" + line + "\"" );
				return false;
			}

			text.Append( "0, 0.000, 0.000, 0.000,\n0, 0.000, 0.000, 0.000,END\n" );

			File.WriteAllText( path + ".str", text.ToString() );
			Console.WriteLine( "Finished exporting string file..." );

			Console.WriteLine( "\nStart writing dtm file..." );
			text.Clear();
			text.Append( Path.GetFileNameWithoutExtension( path ) + ".str,;algorithm=standard;fields=x,y\n" );
			text.Append( "0, 0.000, 0.000, 0.000, END\n" );

			int trisolation = 1;
			int lastOID = -9999;
			foreach ( DTMObject dtm in _DTMList.OrderBy( i => i.objectNumber ) ) {
				if ( lastOID != dtm.objectNumber ) {
					trisolation = 1;
					lastOID = dtm.objectNumber;
				}
				text.Append( "OBJECT, " + dtm.objectNumber + ",\n" );
				text.Append( "TRISOLATION, " + trisolation++ + ", neighbours=no,validate=false,algorithm=legacy\n" );
				int triangleID = 1;
				foreach ( int[] values in dtm.objects.Values ) {
					if ( values.Length < 3 ) {
						Console.WriteLine( "Failure, less than 3 values! Skipping..." );
						Console.WriteLine( values );
						continue;
					}
					text.Append( triangleID + ", " + idDict[values[0]] + ", " + idDict[values[1]] + ", " + idDict[values[2]] + ",\n" );
					triangleID++;
				}
			}

			text.Append( "END\n" );
			File.WriteAllText( path + ".dtm", text.ToString() );

			watch.Stop();
			Console.WriteLine( "Finished exporting dtm file..." );
			Console.WriteLine( "Time: {0} ms", watch.ElapsedMilliseconds );

			return true;
		}

		public bool writeDXFFile( string path )
		{
			Console.WriteLine( "\nStart writing DXF file..." );
			if ( File.Exists( path ) )
				Console.WriteLine( "WARNING: DXF file {0} exists! Overwriting!", path );

			Stopwatch watch = new Stopwatch();
			watch.Start();

			// first: fileheader
			string startText = "   0\nSECTION\n   2\nENTITIES\n   0\n";

			StringBuilder text = new StringBuilder( startText, 4096 );

			int objCount = 0;
			int trisolation = 1;
			int partnumber = 1;
			int lastOID = -9999;
			foreach ( DTMObject dtm in _DTMList.OrderBy( i => i.objectNumber ) ) {
				++objCount;
				Console.Write( "\rCreating Object {0:####} of {1:####} - {2:P0}", objCount, _DTMList.Count, Convert.ToDouble( objCount ) / Convert.ToDouble( _DTMList.Count ) );

				if ( lastOID != dtm.objectNumber ) {
					trisolation = 1;
					lastOID = dtm.objectNumber;
				}

				// Walk through all triangles of an object
				foreach ( int[] values in dtm.objects.Values ) {
					if ( values.Length < 3 ) {
						Console.WriteLine( "Failure, less than 3 values! Skipping..." );
						Console.WriteLine( values );
						continue;
					}

					// New 3DFace
					text.Append( "3DFACE\n   8\n" );
					// first: Objectname -> SURPAC naming convention with ObjectID and Trisolation-Nr
					text.Append( partnumber + "_obj_" + dtm.objectNumber + "_tris_" + trisolation + "\n" );
					// Triangluation points -> doubled 3rd point, because dxf uses 4 point-faces instead of triangles
					text.Append( "  10\n" + _StringDict[values[0]].X + "\n  20\n" + _StringDict[values[0]].Y + "}\n  30\n" + _StringDict[values[0]].Z + "\n" );
					text.Append( "  11\n" + _StringDict[values[1]].X + "\n  21\n" + _StringDict[values[1]].Y + "}\n  31\n" + _StringDict[values[1]].Z + "\n" );
					text.Append( "  12\n" + _StringDict[values[2]].X + "\n  22\n" + _StringDict[values[2]].Y + "}\n  32\n" + _StringDict[values[2]].Z + "\n" );
					text.Append( "  13\n" + _StringDict[values[2]].X + "\n  23\n" + _StringDict[values[2]].Y + "}\n  33\n" + _StringDict[values[2]].Z + "\n   0\n" );
				}
				trisolation++;
				partnumber++;
			}
			text.Append( "ENDSEC\n   0\nEOF\n" );

			Console.WriteLine( "" );

			Console.WriteLine( "Writing text to file..." );
			File.WriteAllText( path, text.ToString() );

			Console.WriteLine( "Finished exporting..." );
			Console.WriteLine( "Time: {0} ms", watch.ElapsedMilliseconds );

			return true;
		}

		public bool writeSKUAFile( string path )
		{
			Console.WriteLine( "\nStart writing SKUA-GOCAD file..." );
			if ( File.Exists( path ) )
				Console.WriteLine( "WARNING: SKUA-GOCAD file {0} exists! Overwriting!", path );

			Stopwatch watch = new Stopwatch();
			watch.Start();

			// first: fileheader
			string startText = "GOCAD TSurf 1\nHEADER {\nname: ";
			string startText2 = "\n*solid*flat: true\n*solid*material_type: Matte\n}\nGOCAD_ORIGINAL_COORDINATE_SYSTEM\nNAME \"gocad Local\"\nAXIS_NAME X Y Z\n";
			startText2 += "AXIS_UNIT m m m\nZPOSITIVE Elevation\nEND_ORIGINAL_COORDINATE_SYSTEM\n";
			startText2 += "PROPERTIES OID\nNO_DATA_VALUES -99999\nPROPERTY_KINDS \"Real Number\"\nUNITS unitless\n";

			StringBuilder text = new StringBuilder( 4096 );
			int objCount = 0;
			foreach ( DTMObject dtm in _DTMList ) {
				++objCount;
				string surfaceName = Path.GetFileNameWithoutExtension( path ) + "_" + ( dtm.objectNumber );
				Console.Write( "\rCreating Object {0:####} of {1:####} - {2:P0}", objCount, _DTMList.Count, Convert.ToDouble( objCount ) / Convert.ToDouble( _DTMList.Count ) );
				text.Append( startText + surfaceName + startText2 );
				List<int> _VertexList = new List<int>();
				foreach ( int id in dtm.objects.Keys ) {
					// test all vertices of the triangle if they are in the vertexlist...
					for ( int i = 0; i < 3; i++ )
						if ( !_VertexList.Contains( dtm.objects[id][i] ) ) {
							try {
								_VertexList.Add( dtm.objects[id][i] );
							}
							catch ( Exception ex ) {
								Console.WriteLine( "An exception occured!" );
								Console.WriteLine( ex.Message );
								foreach ( string key in ex.Data.Keys )
									Console.WriteLine( key + " - " + ex.Data[key] );
								Console.WriteLine( "ID: " + id );
								return false;
							}
						}
				}

				_VertexList.Sort();

				text.Append( "TFACE\n" );
				// add vertices
				for ( int i = 0; i < _VertexList.Count; i++ ) {
					try {
						text.Append( "PVRTX " + ( i + 1 ) + " " + _StringDict[_VertexList[i]].X + " " + _StringDict[_VertexList[i]].Y + " " + _StringDict[_VertexList[i]].Z + " " + dtm.objectNumber + "\n" );
					}
					catch ( Exception ex ) {
						Console.WriteLine( "An exception occured!" );
						Console.WriteLine( ex.Message );
						foreach ( string key in ex.Data.Keys )
							Console.WriteLine( key + " - " + ex.Data[key] );
						Console.WriteLine( "i: " + i );
						Console.WriteLine( "_VertexList[i]: " + _VertexList[i] );

						foreach ( int key in _StringDict.Keys )
							Console.Write( key + " - " );
						return false;
					}
				}

				// add triangles
				foreach ( int id in dtm.objects.Keys ) {
					try {
						text.Append( "TRGL " + ( _VertexList.IndexOf( dtm.objects[id][0] ) + 1 ) );  // + because SKUA-Indexing starts with 1, not with 0
						text.Append( " " + ( _VertexList.IndexOf( dtm.objects[id][1] ) + 1 ) );
						text.Append( " " + ( _VertexList.IndexOf( dtm.objects[id][2] ) + 1 ) + "\n" );
					}
					catch ( Exception ex ) {
						Console.WriteLine( "An exception occured!" );
						Console.WriteLine( ex.Message );
						foreach ( string key in ex.Data.Keys )
							Console.WriteLine( key + " - " + ex.Data[key] );
						Console.WriteLine( "ID: " + id );
						return false;
					}
				}

				text.Append( "END\n" );
			}
			Console.WriteLine( "" );

			Console.WriteLine( "Writing text to file..." );
			File.WriteAllText( path, text.ToString() );

			Console.WriteLine( "Finished exporting..." );
			Console.WriteLine( "Time: {0} ms", watch.ElapsedMilliseconds );

			return true;
		}
	}
}
