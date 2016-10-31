/*
The MIT License(MIT)
Copyright(c) mxgmn 2016.
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
The software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the authors or copyright holders be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or other dealings in the software.
*/

using System.Xml;
using System.Linq;
using System.ComponentModel;

static class Stuff //static keyword make this class be allocated in static area
{
	public static int Random(this double[] a, double r) 
	{
		double sum = a.Sum(); //sum of elements of double type array is stored to sum var

		if (sum == 0) 
		{
			for (int j = 0; j < a.Count(); j++) a[j] = 1; //if sum is 0 then make all of element value to 1
			sum = a.Sum(); // equals number of elements of array
		}

		for (int j = 0; j < a.Count(); j++) a[j] /= sum; // equals 1/length of array

		int i = 0;
		double x = 0;

		while (i < a.Count())//i< length of array
		{
			x += a[i];
			if (r <= x) return i; // function finished
			i++;
		}

		return 0;
	}

	public static long Power(int a, int n) //this function is defined in static area. returns a^n
	{
		long product = 1;
		for (int i = 0; i < n; i++) product *= a;
		return product; //product = a^n
	}

	public static T Get<T>(this XmlNode node, string attribute, T defaultT = default(T)) //template T. this function is defined in static area
	{
		string s = ((XmlElement)node).GetAttribute(attribute);
		var converter = TypeDescriptor.GetConverter(typeof(T));
		return s == "" ? defaultT : (T)converter.ConvertFromInvariantString(s); //if s is qual to empty string then return defaultT else return (T)converter.ConvertFromInvariantString(s)
    }
}
