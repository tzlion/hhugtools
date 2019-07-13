SINTAXINATOR README
===================

Tool to "decrypt" protected Sintax ROM dumps.

The easiest way to use this is in conjunction with a Game Boy emulator having a debugger like BGB.

General usage
-------------

* Select the protected ROM in the "in" field
* Filename will be automatically populated in the "out" field but can be changed if you like
* Click on "Switch" to change between BBD and Sintax mode
* Set up the options as per instructions below
* Check "Open when done" if you want the ROM to be automatically opened in the program associated with the filetype
* Click on "GO!!"

Options
-------

Full Auto
 This automatically processes the ROM based on the values written to the cartridge to initialise the protection.
 If the game is emulated by hhugboy it should be supported by this mode.
 In BGB you can find these values by setting access breakpoints for the specified address ranges and booting the
 protected ROM.
 * Bank scramble mode: For a Sintax game, the value written to 5x1x by the game at boot.
   For a BBD game, the value written to 2x80 by the game at boot.
 * XORs (Sintax mode only): The values written by the game to 7x2x, 7x3x, 7x4x and 7x5x at boot.
 * Bit scramble mode (BBD mode only): the value written to 2x01 by the game at boot.

XOR Bytes (Sintax mode only)
 Manually specify how to XOR the data. I find it easier to figure out how the data is XOR'd by inspecting it in a tile
 editor and looking for large blocks of some repeating pattern at the end of a bank, which likely should be 00 or FF.
 * The first box ("1" by default) denotes how many times to repeat the pattern
 * The second box contains a pattern of XORs to apply to the ROM data, separated by |
   Hex values should be preceded by 0x.
    E.g. 0x12|0x23|0xAF|0x69 will split the ROM into four parts and apply those XORs to each part
    So if you have a 4MB ROM, the first megabyte will be xor'd by 0x12, the second megabyte by 0x23, etc.
   Values WITHOUT an 0x prefix will be taken as a list of bit positions to flip. 
    E.g. 07|146|24567|43 - 146 in this example would flip bits in positions 1, 4 and 6, equivalent to XOR 4A
    The rationale behind this format is if you used a tile editor to inspect the XOR'd data, you can just note down
    which columns are inverted per tile.

Bit reorder (BBD mode only)
 Sequence of bits to reorder in the ROM data, e.g. 01534267

Reorder
 This reorders the banks in the ROM to the order the game expects to see them. Note this is done AFTER the XOR step.
 * Use mode: As per "Bank scramble mode" in full auto
 * Use bank nos: Automatically attempts to reorder using the bank numbers found at the end of each bank.
   Mostly reliable but not 100%.
 * Bit reorder: Sequence of bits to reorder in the bank number, e.g. 07214365

Fix header
 * Automatically fix the ROM header. Usually can be left as-is.
