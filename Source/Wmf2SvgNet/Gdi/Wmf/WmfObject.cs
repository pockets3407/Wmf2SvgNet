namespace Wmf2SvgNet.Gdi.Wmf
{

	public class WmfObject : IGdiObject
	{
		public ushort Id { get; private set; }

		public WmfObject(ushort id)
		{
			Id = id;
		}
	}
}