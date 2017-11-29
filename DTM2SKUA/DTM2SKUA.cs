//
//  DTM2SURFACE.cs
//
//  Author:
//       Stephan Donndorf <stephan.donndorf@googlemail.com>
//
//  Copyright (c) 2016 
//

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using DTMSKUALib;

namespace DTM2SKUA
{
	class DTM2SKUA
	{
		public static void Main( string[] args )
		{
			if ( args.Length < 2 ) {
				Console.WriteLine( "Wrong number of arguments! Canceling..." );
				Console.WriteLine( "Usage: DTM2SKUA <input DTM file> <output SKUA-GOCAD surface file>..." );
				Environment.Exit(-1);
			}

			// change globalization information of this porgram (especially the file formats!)
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo( "en-US" );

			// inform what cannot be stored / converted
			Console.WriteLine( "\n" + new String( '=', 70 ) );
			Console.WriteLine( "This program will convert SURPAC-DTM files to SKUA-GOCAD surface files." );
			Console.WriteLine( "You will loose following information:" );
			Console.WriteLine( "String: string-number" );
			Console.WriteLine( "Style information" );
			Console.WriteLine( "Property information" );
			Console.WriteLine( new String( '=', 70 ) + "\n" );

			try { 
				DTMObjectFile objFile = new DTMObjectFile();
				objFile.readDTMFile( args[0] );
				objFile.writeSKUAFile( args[1] );
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
