// NAnt - A .NET build tool
// Copyright (C) 2001-2003 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Mike Two (2@thoughtworks.com or mike2@nunit.org)
// Tomas Restrepo (tomasr@mvps.org)

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.DotNet.Tasks;
using NAnt.DotNet.Types;
using NAnt.NUnit2.Types;

namespace NAnt.NUnit2OutProc
{
    /// <summary>
    /// Runs tests using the NUnit V2.2 framework.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   The <see cref="HaltOnFailure" /> attribute is only useful when more 
    ///   than one test suite is used, and you want to continue running other 
    ///   test suites although a test failed.
    ///   </para>
    ///   <para>
    ///   Set <see cref="Task.FailOnError" /> to <see langword="false" /> to 
    ///   ignore any errors and continue the build.
    ///   </para>
    ///   <para>
    ///   In order to run a test assembly built with NUnit 2.0 or 2.1 using 
    ///   <see cref="NUnit2OutProcTask" />, you must add the following node to your
    ///   test config file :
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <configuration>
    ///     ...
    ///     <runtime>
    ///         <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
    ///             <dependentAssembly>
    ///                 <assemblyIdentity name="nunit.framework" publicKeyToken="96d09a1eb7f44a77" culture="Neutral" /> 
    ///                 <bindingRedirect oldVersion="2.0.6.0" newVersion="2.2.8.0" /> 
    ///                 <bindingRedirect oldVersion="2.1.4.0" newVersion="2.2.8.0" /> 
    ///             </dependentAssembly>
    ///         </assemblyBinding>
    ///     </runtime>
    ///     ...
    /// </configuration>
    ///     ]]>
    ///   </code>
    ///   <para>
    ///   See the <see href="http://nunit.sf.net">NUnit home page</see> for more 
    ///   information.
    ///   </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Run tests in the <c>MyProject.Tests.dll</c> assembly.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <nunit2outproc>
    ///     <formatter type="Plain" />
    ///     <test assemblyname="MyProject.Tests.dll" appconfig="MyProject.Tests.dll.config" />
    /// </nunit2outproc>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Only run tests that are not known to fail in files listed in the <c>tests.txt</c>
    ///   file.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <nunit2outproc>
    ///     <formatter type="Xml" usefile="true" extension=".xml" outputdir="${build.dir}/results" />
    ///     <test>
    ///         <assemblies>
    ///             <includesfile name="tests.txt" />
    ///         </assemblies>
    ///         <categories>
    ///             <exclude name="NotWorking" />
    ///         </categories>
    ///     </test>
    /// </nunit2outproc>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("nunit2outproc")]
    public class NUnit2OutProcTask : ExecTask
    {
        #region Private Instance Fields

        private bool _haltOnFailure = false;
        private NUnit2TestCollection _tests = new NUnit2TestCollection();
        private FormatterElementCollection _formatterElements = new FormatterElementCollection();

        #endregion Private Instance Fields

        #region Public Instance Properties

        public override bool UseRuntimeEngine
        {
            get { return true; }
        }

        [TaskAttribute("program", Required=false)]
        public new string FileName
        {
            get { return base.FileName; }
//			set { base.FileName = value; }
        }

        /// <summary>
        /// Stop the test run if a test fails. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("haltonfailure")]
        [BooleanValidator()]
        public bool HaltOnFailure
        {
            get { return _haltOnFailure; }
            set { _haltOnFailure = value; }
        }

        /// <summary>
        /// Tests to run.
        /// </summary>
        [BuildElementArray("test")]
        public NUnit2TestCollection Tests
        {
            get { return _tests; }
        }

        /// <summary>
        /// Formatters to output results of unit tests.
        /// </summary>
        [BuildElementArray("formatter")]
        public FormatterElementCollection FormatterElements
        {
            get { return _formatterElements; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        /// <summary>
        /// Runs the tests and sets up the formatters.
        /// </summary>
        protected override void ExecuteTask()
        {
//            if (FormatterElements.Count == 0)
//            {
//                FormatterElement defaultFormatter = new FormatterElement();
//                defaultFormatter.Project = Project;
//                defaultFormatter.NamespaceManager = NamespaceManager;
//                defaultFormatter.Type = FormatterType.Plain;
//                defaultFormatter.UseFile = false;
//                FormatterElements.Add(defaultFormatter);
//
//                Log(Level.Warning, "No <formatter .../> element was specified." +
//                                   " A plain-text formatter was added to prevent losing output of the" +
//                                   " test results.");
//
//                Log(Level.Warning, "Add a <formatter .../> element to the" +
//                                   " <nunit2> task to prevent this warning from being output and" +
//                                   " to ensure forward compatibility with future revisions of NAnt.");
//            }

            string consoleRunnerFilename = CreateConsoleRunner();
            base.FileName = consoleRunnerFilename;

            foreach (NUnit2Test testElement in Tests)
            {
                // include or exclude specific categories
                //string categories = testElement.Categories.Includes.ToString();
                //categories = testElement.Categories.Excludes.ToString();

                foreach (string testAssembly in testElement.TestAssemblies)
                {
                    WorkingDirectory = new FileInfo(testAssembly).Directory;
                    CommandLineArguments = "/noshadow /nologo " + testAssembly;
                    base.ExecuteTask();
                }
            }
        }

        private string CreateConsoleRunner()
        {
            FileInfo thisAssemblyFile = new FileInfo(new Uri(GetType().Assembly.CodeBase).LocalPath);

            FrameworkInfo targetFramework = Project.TargetFramework;
            string externalNUnitDir = (""+Project.Properties["nant.nunit2outproc.nunitpath"]).Trim();

            // assemble directories which can be probed for missing unresolved assembly references
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string libDir = Path.Combine(baseDir, "lib");
            string frameworkFamilyLibDir = Path.Combine(libDir, targetFramework.Family);
            string frameworkVersionLibDir = Path.Combine(frameworkFamilyLibDir, targetFramework.ClrVersion.ToString(2));

            ArrayList probePathList = new ArrayList();
            if (externalNUnitDir.Length > 0)
            {
                // only probe nunit directory
                probePathList.Add(externalNUnitDir);
            }
            else
            {
                // probe nant lib directories
                probePathList.Add(frameworkVersionLibDir);
                probePathList.Add(frameworkFamilyLibDir);
                probePathList.Add(libDir);
                probePathList.Add(baseDir);
            }

            string[] probePaths = (string[]) probePathList.ToArray(typeof(string));
            // find location of nunit.core assembly for current targetFramework
            DirectoryInfo nunitDirectory = ResolveAssemblyLocation("nunit.core", probePaths).Directory;

            //publish nunit-console-runner.dll if not available yet
        	FileInfo nunitConsoleRunnerFile = new FileInfo(Path.Combine(nunitDirectory.FullName, "nunit-console-runner.dll"));
        	PublishAssemblyResourceFile("nunit-console-runner.dll", nunitConsoleRunnerFile );

			// create and compile console-runner.exe using current targetframework if necessary
            string exeFileName = "nunit-console-" + targetFramework.Name + ".exe";
			FileInfo exeFile = new FileInfo( Path.Combine( nunitDirectory.FullName, exeFileName ) );
			if(!exeFile.Exists)
			{
				FileInfo exeConfigFile = new FileInfo(Path.Combine(nunitDirectory.FullName, exeFileName + ".config"));
				PublishAssemblyResourceFile( "nunit-console.exe.config",exeConfigFile );

				string mainExeCode = GetResourceString( "nunit-console.cstemplate" );
				CompileExe(mainExeCode, exeFile, probePaths);
			}

            return exeFile.FullName; // @"D:\NUnit-2.2.8-src\src\build\net\1.0\debug\nunit-console.exe";
        }

		private void CompileExe( string code, FileInfo outputFile, string[] resourcePaths )
		{
			FileInfo exeSourceFile = new FileInfo( Path.GetTempFileName() + ".cs" );
			using(StreamWriter writer = exeSourceFile.CreateText())
			{
				writer.WriteLine(code);
				writer.Close();
			}

			Log( Level.Debug,
				string.Format( "compiling nunit-console at '{0}' to '{1}'",exeSourceFile.FullName,
							  outputFile.FullName ) );

			CscTask csc = new CscTask();
			csc.Parent = this;
			csc.Project = Project;
			csc.NamespaceManager = NamespaceManager;
			csc.InitializeTaskConfiguration();

			csc.OutputFile = outputFile;
			csc.OutputTarget = "exe";
			csc.Debug = false;
			csc.Optimize = true;
			csc.Sources = new FileSet();
			csc.Sources.Includes.Add( exeSourceFile.FullName );
			csc.References = new AssemblyFileSet();
			csc.References.Includes.Add( ResolveAssemblyLocation( "nunit-console-runner", resourcePaths ).FullName );
			csc.Execute();

		}

		/// <summary>
		/// Returns an embedded resource file's content as string
		/// </summary>
		private static string GetResourceString( string resourcename )
		{
			string text = null;
			using(Stream stm = Assembly.GetExecutingAssembly().GetManifestResourceStream( "NAnt.NUnit2OutProc.Resources." + resourcename ))
			{
				StreamReader sr = new StreamReader(stm);
				text = sr.ReadToEnd();
				sr.Close();
			}
			return text;
		}

		private void PublishAssemblyResourceFile( string resourcename, FileInfo outputFile )
		{
			if(!outputFile.Exists)
			{
				Log(Level.Debug, string.Format("copying {0} to {1}", resourcename, outputFile.FullName));
				using(Stream istm = Assembly.GetExecutingAssembly().GetManifestResourceStream( "NAnt.NUnit2OutProc.Resources." + resourcename ))
					using(Stream ostm = new FileStream( outputFile .FullName, FileMode.CreateNew))
					{
						byte[] data = new byte[istm.Length];
						istm.Read(data, 0, data.Length);
						ostm.Write(data, 0, data.Length);
						ostm.Flush();
					}
			}
			else
			{
				Log( Level.Debug,string.Format( "{0} already exists at {1}",resourcename,outputFile.FullName ) );
			}
		}

        private FileInfo ResolveAssemblyLocation(string name, string[] probePaths)
        {
			bool isFullName = name.IndexOf( "Version=" ) != -1;

			Log( Level.Debug, "Resolving assembly " + name );

			// find assembly in probe paths
			foreach(string escapedPath in probePaths)
			{
				string path = escapedPath.TrimStart( '@' ).Trim( '"' );

				if(!Directory.Exists( path ))
				{
					continue;
				}

				Log( Level.Debug, string.Format( "probing path {0} for assembly {1}",path,name ) );

				string[] assemblies = Directory.GetFiles( path,"*.dll" );

				foreach(string assemblyFile in assemblies)
				{
					try
					{
						AssemblyName assemblyName = AssemblyName.GetAssemblyName( assemblyFile );
						if(isFullName)
						{
							if(assemblyName.FullName == name)
							{
								return new FileInfo( assemblyFile );
							}
						}
						else
						{
							if(assemblyName.Name == name)
							{
								return new FileInfo( assemblyFile );
							}
						}
					}
					catch
					{
					}
				}
			}

			// assembly reference could not be resolved
			throw new FileNotFoundException(string.Format("Assembly {0} not found", name), name);
		}

        #endregion Override implementation of Task
    }
}