namespace DereTore.Exchange.Archive.ACB {
    public enum ColumnType : byte {

        Byte = 0x00,
        SByte = 0x01,
        UInt16 = 0x02,
        Int16 = 0x03,
        UInt32 = 0x04,
        Int32 = 0x05,
        UInt64 = 0x06,
        Int64 = 0x07, // not sure
        Single = 0x08,
        Double = 0x09, // not sure
        String = 0x0a,
        Data = 0x0b,
        Mask = 0x0f

    }
}
