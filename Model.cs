/*
The MIT License(MIT)
Copyright(c) mxgmn 2016.
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
The software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the authors or copyright holders be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or other dealings in the software.
*/

using System;

abstract class Model //abstract class can not be instantiated directly; it must be inherited
{
	/* protected keyword: member access is limited to the containing
	class or types derived from the containing class.*/
	protected bool[][][] wave; //3-dimensional array definition
	protected bool[][] changes; //2-dimensinal array definition
	protected double[] stationary; //1-dimensional array def

	protected Random random; //random var
	protected int FMX, FMY, T, limit;
	protected bool periodic;

	double[] logProb; 
	double logT;

	protected abstract bool Propagate(); //abstract function;returns boolean val, not implemented yet.

	bool? Observe()
	{
		double min = 1E+3, sum, mainSum, logSum, noise, entropy; //1E+3 = 10^3
		int argminx = -1, argminy = -1, amount;

		for (int x = 0; x < FMX; x++) for (int y = 0; y < FMY; y++) //nested for loop
			{
				if (OnBoundary(x, y)) continue;//protected abstract bool OnBoundary(int x, int y);

                amount = 0; //initialize to 0
				sum = 0; //initialize to 0

				for (int t = 0; t < T; t++) if (wave[x][y][t]) //if wave[x][y][t] exists
					{
						amount += 1;
						sum += stationary[t];
					}

				if (sum == 0) return false;

                /* from here to below is related to some kind of math calculation related to entropy*/

                noise = 1E-6 * random.NextDouble();
                
                if (amount == 1) entropy = 0;
				else if (amount == T) entropy = logT; //T: (field) int Model.T
				else
				{
					mainSum = 0;
					logSum = Math.Log(sum); //math.Log(x) func returns log of x
					for (int t = 0; t < T; t++) if (wave[x][y][t]) mainSum += stationary[t] * logProb[t];
					entropy = logSum - mainSum / sum;
				}

				if (entropy > 0 && entropy + noise < min) //min stands for minimum
				{
					min = entropy + noise;
					argminx = x; //argminx(argument minimum x) originally initialized to -1
					argminy = y; //argminx(argument minimum y) originally initialized to -1
                }
			}

		if (argminx == -1 && argminy == -1) return true;

		double[] distribution = new double[T];
		for (int t = 0; t < T; t++) distribution[t] = wave[argminx][argminy][t] ? stationary[t] : 0;
        /*statement ? A:B op equals if(statement)A;else B;*/
		int r = distribution.Random(random.NextDouble()); //random number
		for (int t = 0; t < T; t++) wave[argminx][argminy][t] = t == r;
		changes[argminx][argminy] = true;

		return null;
	}

	public bool Run(int seed, int limit) //if Running success returns true
	{
		logT = Math.Log(T);
		logProb = new double[T];
		for (int t = 0; t < T; t++) logProb[t] = Math.Log(stationary[t]);

		Clear();

		random = new Random(seed);

		for (int l = 0; l < limit || limit == 0; l++) //condition : l is less than limit or limit is 0
		{
			bool? result = Observe();
			if (result != null) return (bool)result;
			while (Propagate()); //this holds procedure while Propagate returns true 
		}

		return true;
	}

	protected virtual void Clear()
	{
		for (int x = 0; x < FMX; x++) for (int y = 0; y < FMY; y++) //nested for loop
			{
				for (int t = 0; t < T; t++) wave[x][y][t] = true;
				changes[x][y] = false; //since cleared(no change) returns false
			}
	}

	protected abstract bool OnBoundary(int x, int y);//OverlappingModel.cs - (178, 26) : OverlappingModel.OnBoundary(int, int), checks x and y val are in boundary
    public abstract System.Drawing.Bitmap Graphics();//OverlappingModel.cs - (225, 25) : OverlappingModel.Graphics()
}
