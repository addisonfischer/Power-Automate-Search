using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace paSearch
{
	// Do not forget to update version number and author (company attribute) in AssemblyInfo.cs class
	// To generate Base64 string for Images below, you can use https://www.base64-image.de/
	[Export(typeof(IXrmToolBoxPlugin)),
		ExportMetadata("Name", "Power Automate Search"),
		ExportMetadata("Description", "A simple tool to search Power Automate objects"),
		// Please specify the base64 content of a 32x32 pixels image
		ExportMetadata("SmallImageBase64", "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"),
	// Please specify the base64 content of a 80x80 pixels image
		ExportMetadata("BigImageBase64", "iVBORw0KGgoAAAANSUhEUgAAAFAAAABQCAYAAACOEfKtAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAkESURBVHhe7Zr5jxRFFMebZdGgLLLEePxgjCYoAsqhwh7gKossx+5yqD+Y+Ad4RPFCUfFGPKKigifGRH+RH9V4YlxEDvEAjcefYGKMkd1VUZadWd/39avu6p6amb6qjGa+kzf16nVVz9RnXnVV9+64MZLXUGY1SdlQRjUA5lQDYE41AOZUA2BONQDmVANgTgX7wKlX/MgBO6KPCHab2rYz2IJKyYV2nPxwl6q1LZdDf6yEVhIb8wbfX+QfciQGaAceBieuciJ7dhUL3kRVoKl2YwIPJeChMZVE0Ju1bIF/jLR33Qni2VVTsfAwGBkkU9B8VQ/aRGMAoYxjej+OUR1ZxnUCxvCoLB8jG/XGjg5HmztSzmug9o0j0DQTQNEYtaWYD6zMpd9Ggxbpp53bAzSCx+B8GxsZ9mb1X07HQnU+c0Q8u8oAUA1KLDLQeEwH4oPSzW+jmfFcmJ5UcvYBHs4zSiVZieCV/iJ4Pdwkbp1b7ENsGnjibHFrib5NZFBiQczPqMDX2lUAM/XjWBR2cBwlLxBknHkCD5lXPurN6l0SnsNgnVv+pNKeaiwi9AXwHVji8OCUVCx4EwGWuEGcyniMiyBIircRK0sZXPN8gIA3sy86bWtp/y0nilesYtsYfFHU+M1XKmhaLGG/sKocFUOJzKOSpy3Bw9QlY3grk8NT2n9r8RAjzwOnrv3ed+oO3pRlpAT9wkLFSBX9yDjzMIUVPGQfpu0xbyZP22wqGmIEIDR1zXf0LqHYwJJDi8WCqhY39sObXPe4H655qKvMGyV43WiUS0VCrAAITV39rXj1oEnJhXYcflDV4vF+UOTjsVCoOq55gEdlaYTio96MHJkX1+e3FQPRuI357a059OUBD4Mh01dIdV0K6mQMxBSr0w8WxOg4r7aIIdv8FZfhUfZNO6fYqbfqlWK2OFX3gYffnisDk0HGBx+JCSiO6dAkFq9XxABO7RMBDtmnVtxjXnnkd2/CtPagaxH2yzCc/DJOYV2tfV/Tu2oiJRdat8gpTG1MMfK4nzrmQ8QU5o0yDIsGbZRn9Pf7bSzowO35Mrvunci2dWdipFHDoON1jmnZx1klvtbOzzJ1eZA2vFAAHrKQbs8AT+4yZvT3+W0sWduTf1CZXXUzEGrt/cJ3gqZal0h38oNqGA8/QmvLGYcSMQUPIH2AY6Wj3oy+lX4bBzqwfpJ46ZToXvjwuwtkwDRYDFi3eIwhqesZrEY/rLhB5mGvJw8HaOFwCQ/auA/fKb0SZaBS64r94mHgUgYCLHFV3NDGryLTVCnwaOFQAKf3rsBBZ5p+1nHeGadPYH/TwnFcJlUqgFDr8r3i0XgjXcXnQsWrxIL7W4JHW5bhAXO2zX/8d/HsaklHdCF5JAXE1AChKcv2iCdd64IkcRsyDmFal72hT5ahUlc2QXa3m1fhzYuSQcwEEJrSs1s8SIGBtNMFp0YpbfiaSPAGluNAoPmPRSF9uaFFvFDxNnnVHcu8uJJAzPBA1dfgR10+IGVMB9c0FQMoifNCoWKlCLzJC3d4k9q2q5aBXUywYLoANd4uqy0meKa4bkmUOQOVpiwd0D5NHC5UECUZX/co83aFC8Tkzh204h7xpq++SiLV9ZVkZBxqFlWbtiY9ekntLMycgUqDOxfTO8EJMs8H5ceQkcg+1GPwKPOwUZ6+6sqwXw2beOp53I9BGo4ntcUEj7zEtuEzvFdXboDQ5mun0buCpkzqwV6Ptimils43aa/8N8G7QiL1Netm2cyTft6zVbx0AjwdZlLbsBtjMSv3FFaa0r1TPBLDQ4lTk0/l0G7ckvlqaXvdO7d/jdTS6eu7J3N50eZhLpMK17y8eryrMt8KyUBo8JOl8otJ5pHvP1kp0wyme1tRS/sbmeFlVRHwoDsNmVgYQGhwoIeg0eaYpq1/XwvDHUYI8Ny+1eK5E/+uBdkdn0YhFgoQ4oWCweGRFJ7roaRbNJHpS6UxJdMxk12actFIYus1iIUDhIZ29/Ki4f8Nl+CVQoAudRkvGuRYsPW7fIhWAEJDe9bSBxFAgje5JZr2LmQj8+IGWQMIDe+7mhj+4Z3WFW5XDt5Dq6jp2yQ1JdMxMcBzodspC60ChM5Zcw2Nacybt2lIIhhjttfBjf4WBufS4/qrC5lHEF2ZVYAXbqK9Gn1IYKJDG0+KxhMY91EyHIdd2p7tqXJWPbW4yR5AhhfTvIfDLDx0rwYkhfRz6OrqmGRiatWgwu5EdJng6dLhVQOiFAdtat/lOPOgp7v93Csc4LyHk91ifXNfFMzch6Jg4sd16W0vocxL9xA+v7YIPKhQgEnhKdWCpEsB09s3n3iyN/rnr966j8OHFC70zJLoVa8wgGnh6aoGMp6VkKmtK4hxeFAhAJ/YecR780B4v2tb395fCfEmyxCfNcCDzNGUcgmvmp67fDwtjZQLFqwaPCh3Bs59sHKa2dQiWjSgrUubuYzrxp3F/pj8w9RQrgx0Dg/bFfzcZNe9Y/7bCIOVNnmtHjwoM8A5BM/wmdZsIcHT6+OPn+idMdf836pbe5ojbbNaEmWawnMe+HemrUnbCJZJ45uP8657L/s/UW5dWj/7oNQZ6BoeZx79xNXshg/N17zS6IixfRJLCg9KBdA1vNNOmWCcWnG73gARMVPberatJzk8KPEUnn2/W3gQsi+NXljuT+frP8i2Ej+/LB08KBHAfwNep+MHBC9kgAfVncLMV+W3I/uvwIPqZuDs+wbFc6OO9sr/yrKpF5dnhwfVBHiBY3id/zF4UNUpfMG9BE+bVrato62lYjth01qbR+iD88uYgQzPoVxPW+ilFfmzD6oAeP5Gx/A63MN7uSB4UASga3ht8yd5TePdPpB/eWVx8KDgGugaHjSOxqL/Tdf2q2h4EGfg+fe4h9fmeK+3vdf80CGvmn78qUS/DTLBnf1f4EHjZt79G8bkTG2OV9xXLcKDmowpYsnaaK9nitsy2/Cg1M8Ds2oBZZ5hjNbs1T778KCmHza3GnfqRdoCx5nH5kjBPnDmXYc5ULSQeS71Wr+bzFMy3so1lFzOroH/VzUA5lQDYE41AOZUA2BONQDmVANgLnneP7zPn2/2L0W6AAAAAElFTkSuQmCC"),
	ExportMetadata("BackgroundColor", "Lavender"),
	ExportMetadata("PrimaryFontColor", "Black"),
	ExportMetadata("SecondaryFontColor", "Gray")]
	public class MyPlugin : PluginBase
	{
		public override IXrmToolBoxPluginControl GetControl()
		{
			return new MyPluginControl();
		}

		/// <summary>
		/// Constructor 
		/// </summary>
		public MyPlugin()
		{
			// If you have external assemblies that you need to load, uncomment the following to 
			// hook into the event that will fire when an Assembly fails to resolve
			// AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveEventHandler);
		}

		/// <summary>
		/// Event fired by CLR when an assembly reference fails to load
		/// Assumes that related assemblies will be loaded from a subfolder named the same as the Plugin
		/// For example, a folder named Sample.XrmToolBox.MyPlugin 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private Assembly AssemblyResolveEventHandler(object sender, ResolveEventArgs args)
		{
			Assembly loadAssembly = null;
			Assembly currAssembly = Assembly.GetExecutingAssembly();

			// base name of the assembly that failed to resolve
			var argName = args.Name.Substring(0, args.Name.IndexOf(","));

			// check to see if the failing assembly is one that we reference.
			List<AssemblyName> refAssemblies = currAssembly.GetReferencedAssemblies().ToList();
			var refAssembly = refAssemblies.Where(a => a.Name == argName).FirstOrDefault();

			// if the current unresolved assembly is referenced by our plugin, attempt to load
			if (refAssembly != null)
			{
				// load from the path to this plugin assembly, not host executable
				string dir = Path.GetDirectoryName(currAssembly.Location).ToLower();
				string folder = Path.GetFileNameWithoutExtension(currAssembly.Location);
				dir = Path.Combine(dir, folder);
				var assmbPath = Path.Combine(dir, $"{argName}.dll");
				if (File.Exists(assmbPath))
				{
					loadAssembly = Assembly.LoadFrom(assmbPath);
				}
				else
				{
					throw new FileNotFoundException($"Unable to locate dependency: {assmbPath}");
				}
			}

			return loadAssembly;
		}
	}
}