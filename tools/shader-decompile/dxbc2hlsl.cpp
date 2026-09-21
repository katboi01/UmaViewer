// DXBC -> SPIR-V (DXVK) -> HLSL/MSL (SPIRV-Cross).
// Works on Unity's RDEF-stripped bundle DXBC because DXVK reads resources from the
// SHDR declarations. Resource names come out generic (cb0/cb1, t0..); use the
// reflection from extract_dxbc.py as the legend.
//
// Build: see README.md (links against a locally-built redstrate/dxbc + brew spirv-cross).
//   usage: dxbc2hlsl file.dxbc [msl]     (default output: HLSL SM5.0)
#include <iostream>
#include <fstream>
#include <vector>
#include <dxbc_module.h>
#include <log/log.h>
#include <spirv_cross/spirv_hlsl.hpp>
#include <spirv_cross/spirv_msl.hpp>

dxvk::Logger dxvk::Logger::s_instance("dxbc.log");

int main(int argc, char *argv[]) {
    if (argc < 2) { std::fprintf(stderr, "usage: %s file.dxbc [msl]\n", argv[0]); return 2; }
    std::ifstream infile(argv[1], std::ios_base::binary);
    std::vector<char> buffer((std::istreambuf_iterator<char>(infile)), std::istreambuf_iterator<char>());
    dxvk::DxbcReader reader(buffer.data(), buffer.size());
    dxvk::DxbcModule module(reader);
    dxvk::DxbcModuleInfo info;
    auto result = module.compile(info, "shader");

    bool msl = (argc > 2 && std::string(argv[2]) == "msl");
    if (msl) {
        spirv_cross::CompilerMSL c(result.code.data(), result.code.dwords());
        std::cout << c.compile() << std::endl;
    } else {
        spirv_cross::CompilerHLSL c(result.code.data(), result.code.dwords());
        spirv_cross::CompilerHLSL::Options o; o.shader_model = 50; c.set_hlsl_options(o);
        std::cout << c.compile() << std::endl;
    }
    return 0;
}
