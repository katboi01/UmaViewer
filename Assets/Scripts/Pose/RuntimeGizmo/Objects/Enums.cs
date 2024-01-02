namespace RuntimeGizmos
{
	public enum TransformSpace {Global, Local}
	public enum TransformType {Move, Rotate, Scale /*, RectTool*/, All}
	public enum TransformPivot {Pivot, Center}
	public enum Axis {None, X, Y, Z, Any}

	//CenterType.All is the center of the current object mesh or pivot if not mesh and all its childrens mesh or pivot if no mesh.
	//	CenterType.All might give different results than unity I think because unity only counts empty gameobjects a little bit, as if they have less weight.
	//CenterType.Solo is the center of the current objects mesh or pivot if no mesh.
	//Unity seems to use colliders first to use to find how much weight the object has or something to decide how much it effects the center,
	//but for now we only look at the Renderer.bounds.center, so expect some differences between unity.
	public enum CenterType {All, Solo}

	//ScaleType.FromPoint acts as if you are using a parent transform as your new pivot and transforming that parent instead of the child.
	//ScaleType.FromPointOffset acts as if you are scaling based on a point that is offset from the actual pivot. Its similar to unity editor scaling in Center pivot mode (though a little inaccurate if object is skewed)
	public enum ScaleType {FromPoint, FromPointOffset}
}
