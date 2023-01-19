namespace DereTore.Exchange.Audio.HCA {
    public enum ActionResult {

        HasMoreData = 1,
        Ok = 0,
        InvalidHandle = -1,
        MagicNotMatch = -2,
        AlreadyClosed = -3,
        InvalidParameter = -4,
        InvalidStage = -5,
        FileOpFailed = -6,
        InvalidFileProp = -7,
        InvalidOperation = -8,
        DecodeFailed = -9,
        BufferTooSmall = -10,
        ChecksumNotMatch = -11,
        InvalidInternalState = -12,
        AthInitFailed = -13,
        CiphInitFailed = -14,
        StateOutOfRange = -15,
        NotImplemented = -16,
        DecodeAlreadyCompleted = -17,
        InvalidFieldValue = -18

    }
}
