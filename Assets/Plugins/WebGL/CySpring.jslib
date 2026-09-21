// WebGL no-op stubs for the CySpring native plugin. On WebGL the managed CySpringSolver
// does the work (CySpringNative.isNative is false there); these only satisfy the P/Invoke
// linker for the never-called native path.
mergeInto(LibraryManager.library, {
  NativeClothUpdate: function () {},
  NativeClothSkirtUpdate: function () {},
  NativeSkirtUpdate: function () {},
});
