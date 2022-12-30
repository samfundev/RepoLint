namespace RepoLint.Rules
{
	internal class ImageInSVG : Rule
	{
		public ImageInSVG() : base(".svg") { }

		protected override void Lint()
		{
			if (Content.Contains("<image "))
				Report("<image> elements should be converted to a vector.");
		}
	}
}