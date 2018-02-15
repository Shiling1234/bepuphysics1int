﻿using FixMath.NET;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using BEPUutilities;
using FloatMatrix = BEPUutilitiesFloat.Matrix;
using BEPUtests.util;
using System.Linq;
using System.Diagnostics;
using System;

namespace BEPUtests
{
	public class MatrixTests
    {
		Matrix[] testCases = {
			Matrix.Identity,
			new Matrix(6770.833m, 0, 0, 0, 0, 13500, 0, 0,0, 0, 6770.833m, 0, 0,0,0, 20000m),
			new Matrix(0.6770833m, 0, 0, 0, 0, 1.35m, 0, 0, 0,0, 0.6770833m,  0, 0,0,0, 2m),
			new Matrix(0, 0, 0, 0, 0, 1.35m, 0, 0, 0,0, 0.6770833m,  0, 0,0,0, 2m),
		};

		private readonly ITestOutputHelper output;
		
		public MatrixTests(ITestOutputHelper output)
		{
			if (output == null)
				output = new ConsoleTestOutputHelper();
			this.output = output;
		}


		[Fact]
		public void Invert()
		{
			var maxDelta = 0.00001m;

			var deltas = new List<decimal>();

			// Scalability and edge cases
			foreach (var m in testCases)
			{
				Matrix testCase = m;

				FloatMatrix floatMatrix = MathConverter.Convert(testCase);
				FloatMatrix expected;
				FloatMatrix.Invert(ref floatMatrix, out expected);

				Matrix actual;
				if (float.IsInfinity(expected.M11) || float.IsNaN(expected.M11))
				{
					Assert.Throws<ArgumentException>(() => Matrix.Invert(ref testCase, out actual));
				}
				else
				{
					Matrix.Invert(ref testCase, out actual);
					bool success = true;
					foreach (decimal delta in GetDeltas(expected, actual))
					{
						deltas.Add(delta);
						success &= delta <= maxDelta;

					}
					Assert.True(success, string.Format("Precision: Matrix3x3Invert({0}): Expected {1} Actual {2}", testCase, expected, actual));
				}
			}
			output.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / Fix64.Precision);
			output.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / Fix64.Precision);
		}

		[Fact]
		public void BenchmarkInvert()
		{
			var swf = new Stopwatch();
			var swd = new Stopwatch();

			var deltas = new List<decimal>();

			foreach (var m in testCases)
			{
				Matrix testCase = m;

				for (int i = 0; i < 10000; i++)
				{
					FloatMatrix floatMatrix = MathConverter.Convert(testCase);
					FloatMatrix expected;
					swf.Start();
					FloatMatrix.Invert(ref floatMatrix, out expected);
					swf.Stop();

					Matrix actual;
					swd.Start();
					if (float.IsInfinity(expected.M11) || float.IsNaN(expected.M11))
					{
						Assert.Throws<ArgumentException>(() => Matrix.Invert(ref testCase, out actual));
						swd.Stop();
						continue;
					}
					else
						Matrix.Invert(ref testCase, out actual);
					swd.Stop();

					foreach (decimal delta in GetDeltas(expected, actual))
						deltas.Add(delta);
				}
			}
			output.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / Fix64.Precision);
			output.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / Fix64.Precision);
			output.WriteLine("Fix64.Invert time = {0}ms, float.Invert time = {1}ms", swf.ElapsedMilliseconds, swd.ElapsedMilliseconds);
		}		

		decimal[] GetDeltas(FloatMatrix expected, Matrix actual)
		{
			decimal[] result = new decimal[16];
			int i = 0;
			result[i++] = (decimal)actual.M11 - (decimal)expected.M11;
			result[i++] = (decimal)actual.M12 - (decimal)expected.M12;
			result[i++] = (decimal)actual.M13 - (decimal)expected.M13;
			result[i++] = (decimal)actual.M14 - (decimal)expected.M14;

			result[i++] = (decimal)actual.M21 - (decimal)expected.M21;
			result[i++] = (decimal)actual.M22 - (decimal)expected.M22;
			result[i++] = (decimal)actual.M23 - (decimal)expected.M23;
			result[i++] = (decimal)actual.M24 - (decimal)expected.M24;

			result[i++] = (decimal)actual.M31 - (decimal)expected.M31;
			result[i++] = (decimal)actual.M32 - (decimal)expected.M32;
			result[i++] = (decimal)actual.M33 - (decimal)expected.M33;
			result[i++] = (decimal)actual.M34 - (decimal)expected.M34;

			result[i++] = (decimal)actual.M41 - (decimal)expected.M41;
			result[i++] = (decimal)actual.M42 - (decimal)expected.M42;
			result[i++] = (decimal)actual.M43 - (decimal)expected.M43;
			result[i++] = (decimal)actual.M44 - (decimal)expected.M44;

			return result;
		}
	}
}