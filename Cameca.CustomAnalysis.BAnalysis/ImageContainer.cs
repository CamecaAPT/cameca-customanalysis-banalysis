using System;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cameca.CustomAnalysis.BAnalysis;

internal static class ImageContainer
{
	public static ImageSource BAnalysisIcon { get; } = CreateAndCheckUriString("BAnalysisIcon.png");

	/// <summary>
	/// Get an ImageSource from URI string
	/// </summary>
	/// <param name="imageName">Image filename</param>
	/// <param name="folderName">Path to images in assembly</param>
	/// <returns></returns>
	private static ImageSource CreateAndCheckUriString(string imageName, string folderName = "Images")
	{
		string path = $"/{folderName}/{imageName}";
		Uri uri = GetResourceUri(Assembly.GetCallingAssembly(), path);
		var image = new BitmapImage(uri);
		image.Freeze();
		return image;
	}

	private static Uri GetResourceUri(Assembly assembly, string path)
	{
		return new Uri($"pack://application:,,,/{assembly.GetName().Name};component/{path}");
	}
}
