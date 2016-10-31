/*
The MIT License(MIT)
Copyright(c) mxgmn 2016.
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
The software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the authors or copyright holders be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or other dealings in the software.
*/

using System; //using System namespace
using System.Xml; //using System.Xml namespace

static class Program /*static class -which name is Program- define. static
keyword make this class be stored in static area of memory*/
{
	static void Main() //main function start from here
	{
		Random random = new Random(); //Random type variable
		var xdoc = new XmlDocument(); //var xdoc holds xmlDocument
		xdoc.Load("samples.xml"); //open "samples.xml" file and put this into xdoc

		int counter = 1;
		foreach (XmlNode xnode in xdoc.FirstChild.ChildNodes)
		/*parsing sample.xml node by node by foreach loop*/
		{
			if (xnode.Name == "#comment") continue; //if current node from xml is comment, pass it and continue

			Model model; //Model type var
			string name = xnode.Get<string>("name"); //name name from xml
			Console.WriteLine($"< {name}"); //display name

			if (xnode.Name == "overlapping") model = new OverlappingModel(name, xnode.Get("N", 2), xnode.Get("width", 48), xnode.Get("height", 48),//if name is overlapping then make OverlapplingModel
				xnode.Get("periodicInput", true), xnode.Get("periodic", false), xnode.Get("symmetry", 8), xnode.Get("ground", 0));
			else if (xnode.Name == "simpletiled") model = new SimpleTiledModel(name, xnode.Get<string>("subset"), //else if name is simpletiled then make SimpleTileModel
				xnode.Get("width", 10), xnode.Get("height", 10), xnode.Get("periodic", false), xnode.Get("black", false));
			else continue; //if Name is none of 'overlapping' or 'simpletiled' pass it and continue

			for (int i = 0; i < xnode.Get("screenshots", 2); i++) //in this statement, loop condition is that i is less then 2 and success getting screenshot
			{
				for (int k = 0; k < 10; k++) // loop 10 times
				{
					Console.Write("> ");
					int seed = random.Next();//seed to generate random number.
					bool finished = model.Run(seed, xnode.Get("limit", 0));
					if (finished) //val of finish is nonzero which means model.Run gets to limit
					{
						Console.WriteLine("DONE"); //display "DONE"

						model.Graphics().Save($"{counter} {name} {i}.png");//save model graphic to png file
						if (model is SimpleTiledModel && xnode.Get("textOutput", false))//if failed to save, throw exception
							System.IO.File.WriteAllText($"{counter} {name} {i}.txt", (model as SimpleTiledModel).TextOutput());

						break;
					}
					else Console.WriteLine("CONTRADICTION");//if success saving, display "CONTRADICTION"
				}
			}

			counter++;
		}
	}
}
