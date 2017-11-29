//
//  Program.cs
//
//  Author:
//       Stephan Donndorf <stephan.donndorf@googlemail.com>
//
//  Copyright (c) 2016 Stephan Donndorf
//

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using DTMSKUALib;

namespace SKUA2DXF
{
	class SKUA2DXF
	{
		public static void Main( string[] args )
		{
			if ( args.Length < 2 ) {
				Console.WriteLine( "Wrong number of arguments! Canceling..." );
				Console.WriteLine( "Usage: SKUA2DTM <SKUA-GOCAD surface> <output DXF-file>..." );
				Environment.Exit( -1 );
			}

			// change globalization information of this porgram (especially the file formats!)
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo( "en-US" );

			// inform what cannot be stored / converted
			Console.WriteLine( "\n" + new String( '=', 70 ) );
			Console.WriteLine( "This program will convert SKUA-GOCAD surfaces to DXF files." );
			Console.WriteLine( "You will loose following information:" );
			Console.WriteLine( "Part information" );
			Console.WriteLine( "Style information" );
			Console.WriteLine( "Property information" );
			Console.WriteLine( new String( '=', 70 ) + "\n" );

			try {
				DTMObjectFile objFile = new DTMObjectFile();
				objFile.readSKUAFile( args[0] );
				objFile.writeDXFFile( args[1] );
			}
			catch ( FileNotFoundException ex ) {
				Console.WriteLine( "Cannot read input file!" );
				Console.WriteLine( ex.Message );
				foreach ( string key in ex.Data.Keys )
					Console.WriteLine( key + " - " + ex.Data[key] );
			}
		}
	}
}