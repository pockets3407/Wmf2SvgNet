using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wmf2SvgNet.Gdi.Svg;
using Wmf2SvgNet.Gdi.Wmf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wmf2SvgNet.Gdi.Wmf.Tests
{
    [TestClass()]
    public class WmfParserTests
    {
        [TestMethod()]
        public void ConvertToSvgTest()
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ??
                throw new NullReferenceException("Could not get information about the executing assembly");

            string testFile = Path.Combine(path, "TestData", "hellopeople.wmf");

            Stream input = new FileStream(testFile, FileMode.Open);
            WmfParser parser = new WmfParser();
            SvgGdi gdi = new SvgGdi(false, true);
            gdi.ReplaceSymbolFont = false;

            parser.Parse(input, gdi);


            Stream output = null;
            try
            {
                output = new FileStream(Path.ChangeExtension(testFile, "svg"), FileMode.Create);

                gdi.Write(output);
            }
            finally
            {
                if (output != null)
                    output.Close();
            }

            Assert.IsTrue(true);
        }
    }
}