using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace Lab_6
{	
	class MatrixFilter {
		
		private double[,] filter;
		
		public MatrixFilter(double [,] matrix) {
			filter = matrix;
		}
		
		public Bitmap applyFilter(Bitmap sourceImage) {
			Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height, 
			                                sourceImage.PixelFormat);
			int h = resultImage.Height,
				w = resultImage.Width,
				r = ((int)Math.Sqrt(filter.Length) - 1) / 2;
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					double Red = 0, Blue = 0, Green = 0;
					for (int n = x - r; n <= x + r; n++) {
						int i = n;
						if (i < 0)
							i = 0;
						if (i >= w)
							i = w - 1;
						for (int m = y - r; m <= y + r; m++) {
							int j = m;
							if (j < 0)
								j = 0;
							if (j >= h)
								j = h - 1;
							
							Color currentColor = sourceImage.GetPixel(i, j);
							Red += currentColor.R * filter[n - x + r, m - y + r];
							Green += currentColor.G * filter[n - x + r, m - y + r];
							Blue += currentColor.B * filter[n - x + r, m - y + r];
						}
					}
					Color fillColor = Color.FromArgb((int)Red, (int)Green, (int)Blue);
					resultImage.SetPixel(x, y, fillColor);
				}
			}
			return resultImage;
		}
		
		public unsafe Bitmap applyFilterUnsafe(Bitmap sourceImage) {
			Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height, 
			                                sourceImage.PixelFormat);
			int h = resultImage.Height,
				w = resultImage.Width,
				r = ((int)Math.Sqrt(filter.Length) - 1) / 2;
			BitmapData sourceData = sourceImage.LockBits(
					new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), 
					ImageLockMode.ReadOnly, sourceImage.PixelFormat),
			resultData = resultImage.LockBits(
					new Rectangle(0, 0, resultImage.Width, resultImage.Height),
					ImageLockMode.WriteOnly, resultImage.PixelFormat);
			
			int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(sourceImage.PixelFormat) / 8;
			
			int heightInPixels = sourceData.Height;
			int widthInBytes = sourceData.Width * bytesPerPixel;
			int stride = sourceData.Stride;
			byte* sourcePtrFirstPixel = (byte*)sourceData.Scan0;
			byte* resultPtrFirstPixel = (byte*)resultData.Scan0;
			
			for (int y = 0; y < heightInPixels; y++) {
				byte* sourceCurrentLine = sourcePtrFirstPixel + (y * stride);
				byte* resultCurrentLine = resultPtrFirstPixel + (y * stride);
				for (int x = 0; x < widthInBytes; x = x + bytesPerPixel) {
					
					double Red = 0, Blue = 0, Green = 0;
					for (int n = x / bytesPerPixel - r; n <= x / bytesPerPixel + r; n++) {
						int i = n;
						if (i < 0)
							i = 0;
						if (i >= w)
							i = w - 1;
						for (int m = y - r; m <= y + r; m++) {
							int j = m;
							if (j < 0)
								j = 0;
							if (j >= h)
								j = h - 1;
							
							byte* currentPixel = sourcePtrFirstPixel
												 + j * stride 
												 + i * bytesPerPixel;
							Red   += currentPixel[0] * filter[n - x/bytesPerPixel + r, m - y + r];
							Green += currentPixel[1] * filter[n - x/bytesPerPixel + r, m - y + r];
							Blue  += currentPixel[2] * filter[n - x/bytesPerPixel + r, m - y + r];
						}
					}
					resultCurrentLine[x] = (byte)Red;
					resultCurrentLine[x + 1] = (byte)Green;
					resultCurrentLine[x + 2] = (byte)Blue;
				}
			}
			sourceImage.UnlockBits(sourceData);
			resultImage.UnlockBits(resultData);
			return resultImage;
		}
			
		public unsafe Bitmap applyFilterUnsafeParallel(Bitmap sourceImage) {
			Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height, 
			                                sourceImage.PixelFormat);
			int h = resultImage.Height,
				w = resultImage.Width,
				r = ((int)Math.Sqrt(filter.Length) - 1) / 2;
			BitmapData sourceData = sourceImage.LockBits(
					new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), 
					ImageLockMode.ReadOnly, sourceImage.PixelFormat),
			resultData = resultImage.LockBits(
					new Rectangle(0, 0, resultImage.Width, resultImage.Height),
					ImageLockMode.WriteOnly, resultImage.PixelFormat);
			
			int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(sourceImage.PixelFormat) / 8;
			
			int heightInPixels = sourceData.Height;
			int widthInBytes = sourceData.Width * bytesPerPixel;
			int stride = sourceData.Stride;
			byte* sourcePtrFirstPixel = (byte*)sourceData.Scan0;
			byte* resultPtrFirstPixel = (byte*)resultData.Scan0;
			
			Parallel.For(0, heightInPixels, y => {
				byte* sourceCurrentLine = sourcePtrFirstPixel + (y * stride);
				byte* resultCurrentLine = resultPtrFirstPixel + (y * stride);
				for (int x = 0; x < widthInBytes; x = x + bytesPerPixel) {
					
					double Red = 0, Blue = 0, Green = 0;
					for (int n = x / bytesPerPixel - r; n <= x / bytesPerPixel + r; n++) {
						int i = n;
						if (i < 0)
							i = 0;
						if (i >= w)
							i = w - 1;
						for (int m = y - r; m <= y + r; m++) {
							int j = m;
							if (j < 0)
								j = 0;
							if (j >= h)
								j = h - 1;
							
							byte* currentPixel = sourcePtrFirstPixel
												 + j * stride 
												 + i * bytesPerPixel;
							Red   += currentPixel[0] * filter[n - x/bytesPerPixel + r, m - y + r];
							Green += currentPixel[1] * filter[n - x/bytesPerPixel + r, m - y + r];
							Blue  += currentPixel[2] * filter[n - x/bytesPerPixel + r, m - y + r];
						}
					}
					resultCurrentLine[x] = (byte)Red;
					resultCurrentLine[x + 1] = (byte)Green;
					resultCurrentLine[x + 2] = (byte)Blue;
				}
			});
			sourceImage.UnlockBits(sourceData);
			resultImage.UnlockBits(resultData);
			return resultImage;
		}
		
	}
	
	class Program
	{
		
		private static double [,] GaussMatrix(int size) {
			double [,] matrix = new double[size, size];
			int radius = size / 2;
			double sigma = (double)radius / 3;
			double sumTotal = 0;
            for (int i = -radius; i <= radius; i++) {
				for (int j = -radius; j <= radius; j++) {
					matrix[i + radius, j + radius] = (1 / (2 * Math.PI * Math.Pow(sigma, 2))) * 
						Math.Exp(-(Math.Pow(i, 2) + Math.Pow(j, 2) / (2 * Math.Pow(sigma, 2))));
                    sumTotal += matrix[i + radius, j + radius];
                }
            }
            for (int i = 0; i < size; i++) {
				for (int j = 0; j < size; j++) {
                    matrix[i, j] /= sumTotal;
                }
            }
			return matrix;
		}
		
		public static void Main(string[] args)
		{
			
			int K = 3;
			MatrixFilter mf = new MatrixFilter(GaussMatrix(K));
			DateTime start;
			TimeSpan elapsed;
			
			string folder = "tests/", 
				filename = "photo", 
				extension = ".jpg";
			
			Bitmap img = new Bitmap(folder + filename + extension);
			Console.WriteLine("h = {0} w = {1}", img.Height, img.Width);
			
			start = DateTime.Now;
			Bitmap new_img = mf.applyFilter(img);
			elapsed = DateTime.Now - start;
			Console.WriteLine("Get/SetPixel time = {0}ms", elapsed.TotalMilliseconds);
			new_img.Save(folder + filename + "_getset_filter(" + K + ")" + ".png");
	
			start = DateTime.Now;
			Bitmap new_unsafe_img = mf.applyFilterUnsafe(img);
			elapsed = DateTime.Now - start;
			Console.WriteLine("Unsafe time = {0}ms", elapsed.TotalMilliseconds);
			new_unsafe_img.Save(folder + filename + "_unsafe_filter(" + K + ")" + ".png");
			
			start = DateTime.Now;
			Bitmap new_unsafe_img_parallel = mf.applyFilterUnsafeParallel(img);
			TimeSpan elapsedParallel = DateTime.Now - start;
			new_unsafe_img_parallel.Save(folder + filename + "_unsafe_parallel_filter(" + K + ")" + ".png");
			Console.WriteLine("Parallel Time = {0}ms", elapsedParallel.TotalMilliseconds);
			
			Console.WriteLine("SpeedUp = {0:0.##}", elapsed.TotalMilliseconds / elapsedParallel.TotalMilliseconds);
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}