module compiler.linker;

import compiler.compiler;
import compiler.config;

import std.stdio;
import std.file;

version (linux)
{
    import core.sys.linux.elf;

    enum int elf_address_base = 0x400000;
    enum int elf_header_size  = (Elf64_Ehdr.sizeof + (Elf64_Phdr.sizeof));

    // Link to ELF
    void link_to_elf(Compiler *compiler)
    {
        Elf64_Ehdr    header;
        Elf64_Phdr[2] programs;

        header.e_ident[0] = 0x7F;
        header.e_ident[1] = 0x45;
        header.e_ident[2] = 0x4C;
        header.e_ident[3] = 0x46;
        header.e_ident[4] = 2;
        header.e_ident[5] = 1;
        header.e_ident[6] = 1;
        header.e_ident[7] = 0;
        header.e_ident[8] = 0;

        header.e_type      = 2;
        header.e_machine   = 62;
        header.e_version   = 1;
        header.e_entry     = (elf_address_base + elf_header_size + 0 /* entry point */);
        header.e_phoff     = Elf64_Ehdr.sizeof;
        header.e_shoff     = 0;
        header.e_flags     = 0;
        header.e_ehsize    = Elf64_Ehdr.sizeof;
        header.e_phentsize = Elf64_Phdr.sizeof;
        header.e_phnum     = 1;
        header.e_shentsize = Elf64_Shdr.sizeof;
        header.e_shnum     = 0;
        header.e_shstrndx  = 0;

        programs[0].p_type   = 1;
        programs[0].p_offset = 0;
        programs[0].p_vaddr  = elf_address_base;
        programs[0].p_paddr  = elf_address_base;
        programs[0].p_filesz = (elf_header_size + compiler.x64_backend.assembler.code.length);
        programs[0].p_memsz  = (elf_header_size + compiler.x64_backend.assembler.code.length);
        programs[0].p_flags  = 5;
        programs[0].p_align  = 0x1000;

        // programs[1].p_type   = 1;
        // programs[1].p_offset = elf_header_size;
        // programs[1].p_vaddr  = (elf_address_base + 0x1000 + elf_header_size + compiler.x64_backend.assembler.code.length);
        // programs[1].p_paddr  = (elf_address_base + 0x1000 + elf_header_size + compiler.x64_backend.assembler.code.length);
        // programs[1].p_filesz = 0;
        // programs[1].p_memsz  = 0;
        // programs[1].p_flags  = 6;
        // programs[1].p_align  = 0x1000;

        File file = File("tests/out/a.out", "w");
        fwrite(&header, byte.sizeof, header.sizeof, file.getFP());
        fwrite(&programs[0], byte.sizeof, programs[0].sizeof, file.getFP());
        // fwrite(&programs[1], byte.sizeof, programs[1].sizeof, file.getFP());
        fwrite(compiler.x64_backend.assembler.code.ptr, byte.sizeof, compiler.x64_backend.assembler.code.length, file.getFP());
        file.close();
    }
}

// Link
void link(Compiler *compiler)
{
    final switch (g_backend)
    {
        case backend_x64:
        {
            version (linux)
                link_to_elf(compiler);
            break;
        }
    }
}