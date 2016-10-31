/*
The MIT License(MIT)
Copyright(c) mxgmn 2016.
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
The software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the authors or copyright holders be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or other dealings in the software.
*/

using System;
using System.Xml;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class SimpleTiledModel : Model //SimpleTiledModel 
{
	bool[][][] propagator; //3-d boolean array

	List<Color[]> tiles; //List data structure for color array type
	List<string> tilenames;
	int tilesize; //this will store size of tile
	bool black;

	public SimpleTiledModel(string name, string subsetName, int width, int height, bool periodic, bool black)
	{
		FMX = width; //FMX from Model
		FMY = height; //FMY from Model
		this.periodic = periodic;
		this.black = black; //this.black is var of this class, while black is arg

		var xdoc = new XmlDocument(); //allocate XmlDocument var
		xdoc.Load($"samples/{name}/data.xml");
		XmlNode xnode = xdoc.FirstChild; //xnode (iterator role) is initialized to fitstchild of xdoc
		tilesize = xnode.Get("size", 16);
		bool unique = xnode.Get("unique", false); //get unique from xnode and store to unique var
		xnode = xnode.FirstChild;

		List<string> subset = null;
		if (subsetName != default(string)) //if subsetName is not same as default string
		{
			subset = new List<string>(); //allocate List for sring type
			foreach (XmlNode xsubset in xnode.NextSibling.NextSibling.ChildNodes) 
				if (xsubset.NodeType != XmlNodeType.Comment && xsubset.Get<string>("name") == subsetName)
					foreach (XmlNode stile in xsubset.ChildNodes) subset.Add(stile.Get<string>("name")); //only iterate  when if state above is true
		}

		Func<Func<int, int, Color>, Color[]> tile = f =>
		{
			Color[] result = new Color[tilesize * tilesize];
			for (int y = 0; y < tilesize; y++) for (int x = 0; x < tilesize; x++) result[x + y * tilesize] = f(x, y); //nested for loop
			return result;
		};

		Func<Color[], Color[]> rotate = array => tile((x, y) => array[tilesize - 1 - y + x * tilesize]); //function delegate which have 2 parameters

		tiles = new List<Color[]>(); //tiles is initialized
		tilenames = new List<string>();
		var tempStationary = new List<double>();//temperary station

		List<int[]> action = new List<int[]>(); //int array List is allocated
		Dictionary<string, int> firstOccurrence = new Dictionary<string, int>();

		foreach (XmlNode xtile in xnode.ChildNodes)
		{
			string tilename = xtile.Get<string>("name");
			if (subset != null && !subset.Contains(tilename)) continue; //contiue keyword make procedure to go to the end of the block

			Func<int, int> a, b; //define func<t1,T2> delegate a,b
			int cardinality;

			char sym = xtile.Get("symmetry", 'X');
			if (sym == 'L') //if sym is L
			{
				cardinality = 4; //cardinality is set to 4
				a = i => (i + 1) % 4;
				b = i => i % 2 == 0 ? i + 1 : i - 1; //function delegate is implemented
			}
			else if (sym == 'T') //if sym is not L and sym is T
			{
				cardinality = 4; //cardinality is set to 4
				a = i => (i + 1) % 4;
				b = i => i % 2 == 0 ? i : 4 - i; //function delegate is implemented
			}
			else if (sym == 'I') //if sym is I
			{
				cardinality = 2; //cardinality is set to 2
				a = i => 1 - i;
				b = i => i; //functiondelegate is implemented; i is just returned
			}
			else if (sym == '\\') //if sym is \\(two back slash)
			{
				cardinality = 2; //cardinality is set to 2
				a = i => 1 - i;
				b = i => 1 - i; // function delegate implementation
			}
			else //in other case
			{
				cardinality = 1; //cardinality is set to 1
				a = i => i;
				b = i => i; //function delegate implementation
			}

			T = action.Count;
			firstOccurrence.Add(tilename, T); //add (tilename, T) to firstOccurrence
			
			int[][] map = new int[cardinality][]; //allocate 2-d int array. row is fixed
			for (int t = 0; t < cardinality; t++)
			{
				map[t] = new int[8];

				map[t][0] = t;
				map[t][1] = a(t); //a is delegator
				map[t][2] = a(a(t));
				map[t][3] = a(a(a(t))); //function composition
				map[t][4] = b(t);
				map[t][5] = b(a(t)); //function composition
				map[t][6] = b(a(a(t)));
				map[t][7] = b(a(a(a(t)))); //function composition

				for (int s = 0; s < 8; s++) map[t][s] += T; //map[t][s] = map[t][s] + T

				action.Add(map[t]); //List<int[]> action; add map[t] to action
            }

			if (unique)
			{
				for (int t = 0; t < cardinality; t++)
				{
					Bitmap bitmap = new Bitmap($"samples/{name}/{tilename} {t}.png"); //allocate new Bitmap 
					tiles.Add(tile((x, y) => bitmap.GetPixel(x, y)));
					tilenames.Add($"{tilename} {t}"); //Add new element to tilenames(List)
				}
			}
			else
			{
				Bitmap bitmap = new Bitmap($"samples/{name}/{tilename}.png");
				tiles.Add(tile((x, y) => bitmap.GetPixel(x, y)));
				tilenames.Add($"{tilename} 0");

				for (int t = 1; t < cardinality; t++)
				{
					tiles.Add(rotate(tiles[T + t - 1]));
					tilenames.Add($"{tilename} {t}");
				}
			}

			for (int t = 0; t < cardinality; t++) tempStationary.Add(xtile.Get("weight", 1.0f));//definition : var tempStationary = new List<double>()
        }
        
		T = action.Count;
		stationary = tempStationary.ToArray(); //convert tempstationary(List) to array type and assign to stationary

		propagator = new bool[4][][]; //initialize propagator to 3-d bool array, first demension is fixed
		for (int d = 0; d < 4; d++)
		{
			propagator[d] = new bool[T][];
			for (int t = 0; t < T; t++) propagator[d][t] = new bool[T]; //nested for loop; expands propagator[][] to array iteratively
		}

		wave = new bool[FMX][][];
		changes = new bool[FMX][]; //initialization
		for (int x = 0; x < FMX; x++)
		{
			wave[x] = new bool[FMY][];
			changes[x] = new bool[FMY]; //array expansion
			for (int y = 0; y < FMY; y++) wave[x][y] = new bool[T];
		}

		foreach (XmlNode xneighbor in xnode.NextSibling.ChildNodes)//xneighbor iterates nodes of xnode(xml file)
		{
			string[] left = xneighbor.Get<string>("left").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			string[] right = xneighbor.Get<string>("right").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (subset != null && (!subset.Contains(left[0]) || !subset.Contains(right[0]))) continue; //go to the end of the block

			int L = action[firstOccurrence[left[0]]][left.Length == 1 ? 0 : int.Parse(left[1])], D = action[L][1]; //if action[firstOccurrence[left[0]]][left.Length == 1 then int.Parse(left[1])] else D
            int R = action[firstOccurrence[right[0]]][right.Length == 1 ? 0 : int.Parse(right[1])], U = action[R][1];
            /*sets propagator value*/
			propagator[0][R][L] = true;
			propagator[0][action[R][6]][action[L][6]] = true; // val of action[][] must be int
			propagator[0][action[L][4]][action[R][4]] = true;
			propagator[0][action[L][2]][action[R][2]] = true;

			propagator[1][U][D] = true;
			propagator[1][action[D][6]][action[U][6]] = true;
			propagator[1][action[U][4]][action[D][4]] = true;
			propagator[1][action[D][2]][action[U][2]] = true;
		}

		for (int t2 = 0; t2 < T; t2++) for (int t1 = 0; t1 < T; t1++) //nested loop
			{
				propagator[2][t2][t1] = propagator[0][t1][t2]; //assign R value(propagator[0][t1][t2]) to L value(propagator[2][t2][t1])
                propagator[3][t2][t1] = propagator[1][t1][t2];
			}
	}

	protected override bool Propagate() //redefine Propagate function
	{
		bool change = false, b;
		for (int x2 = 0; x2 < FMX; x2++) for (int y2 = 0; y2 < FMY; y2++) for (int d = 0; d < 4; d++) //3 nested loop
				{
					int x1 = x2, y1 = y2; //define x1,y1 and assign value of x2, y2
                    
                    /*below codes there are mainly 4 cases: d ==0, d==1, d==2, or else */
					if (d == 0)
					{
						if (x2 == 0)
						{
							if (!periodic) continue; //go to the end of the block, in this case, end of for loop
							else x1 = FMX - 1;
						}
						else x1 = x2 - 1;
					}
					else if (d == 1)
					{
						if (y2 == FMY - 1)
						{
							if (!periodic) continue; //go to the end of the block, in this case, end of for loop
                            else y1 = 0;
						}
						else y1 = y2 + 1;
					}
					else if (d == 2)
					{
						if (x2 == FMX - 1)
						{
							if (!periodic) continue;//go to the end of the block, in this case, end of for loop
                            else x1 = 0;
						}
						else x1 = x2 + 1;
					}
					else
					{
						if (y2 == 0)
						{
							if (!periodic) continue;//go to the end of the block, in this case, end of for loop
                            else y1 = FMY - 1;
						}
						else y1 = y2 - 1;
					}

					if (!changes[x1][y1]) continue;//go to the end of the block, in this case, end of for loop

                    bool[] w1 = wave[x1][y1]; //wave[][] must be 1-d bool array
					bool[] w2 = wave[x2][y2];

					for (int t2 = 0; t2 < T; t2++) if (w2[t2])
						{
							bool[] prop = propagator[d][t2];//definition: bool[][][] propagator
                            b = false;
                        
							for (int t1 = 0; t1 < T && !b; t1++) if (w1[t1]) b = prop[t1];
							if (!b) //if b is false
							{
								wave[x2][y2][t2] = false;
								changes[x2][y2] = true; //change occur
								change = true; //change occur
							}
						}
				}			

		return change; //after the procedure(propergate) if change occurs returns true else returns false
	}

	protected override bool OnBoundary(int x, int y) => false;

	public override Bitmap Graphics()
	{
		Bitmap result = new Bitmap(FMX * tilesize, FMY * tilesize);
		int[] bitmapData = new int[result.Height * result.Width]; //allocate new int array to bitmapData

		for (int x = 0; x < FMX; x++) for (int y = 0; y < FMY; y++) //nested forloop : first, y val is changes x val later
			{
				bool[] a = wave[x][y]; //protected bool[][][] wave from class Model

                int amount = (from b in a where b select 1).Sum(); //sum of a where b select 1 is stored in amount var
                double lambda = 1.0 / (from t in Enumerable.Range(0, T) where a[t] select stationary[t]).Sum();

				for (int yt = 0; yt < tilesize; yt++) for (int xt = 0; xt < tilesize; xt++)
					{
						if (black && amount == T) bitmapData[x * tilesize + xt + (y * tilesize + yt) * FMX * tilesize] = unchecked((int)0xff000000); //at least black is false then go to else
						else
						{
							double r = 0, g = 0, b = 0;
							for (int t = 0; t < T; t++) if (wave[x][y][t])//if this statement is false then continue;
								{
									Color c = tiles[t][xt + yt * tilesize];
									r += (double)c.R * stationary[t] * lambda; //amount and lamda val is not changed while looping,
									g += (double)c.G * stationary[t] * lambda;
									b += (double)c.B * stationary[t] * lambda; //r, g, b is related to RGB value of pixel
								}

							bitmapData[x * tilesize + xt + (y * tilesize + yt) * FMX * tilesize] = 
								unchecked((int)0xff000000 | ((int)r << 16) | ((int)g << 8) | (int)b);
						}
					}
			}

		var bits = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);//lock bit before system call
		System.Runtime.InteropServices.Marshal.Copy(bitmapData, 0, bits.Scan0, bitmapData.Length);
		result.UnlockBits(bits); //unlock bit after system call

		return result;
	}

	public string TextOutput() //returns string type. public accessability: can be accessed from outside of class
	{
		var result = new System.Text.StringBuilder(); //initialize new instance of StringBuilder, result

		for (int y = 0; y < FMY; y++) //FMY from Model class
		{
			for (int x = 0; x < FMX; x++) //FMX from Model class
				for (int t = 0; t < T; t++) if (wave[x][y][t])
					{
						result.Append($"{tilenames[t]}, ");//append $"{tilenames[t]}, " to the end of result string
                        break; //exit the current for loop
					}

			result.Append(Environment.NewLine); //append newLine to the result string
		}

		return result.ToString(); //convert result to String and output 
	}
}
