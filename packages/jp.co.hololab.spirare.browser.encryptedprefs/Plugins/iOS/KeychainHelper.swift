import Foundation

public class KeychainHelper {
    static func save(key: String, data: String) -> Bool {
        guard let dataToSave = data.data(using: .utf8) else { return false }

        let query = [
            kSecClass as String: kSecClassGenericPassword as String,
            kSecAttrAccount as String: key,
            kSecValueData as String: dataToSave ] as [String : Any]

        SecItemDelete(query as CFDictionary)
        let result = SecItemAdd(query as CFDictionary, nil)
        return result == noErr
    }

    static func load(key: String) -> String? {
        let query = [
            kSecClass as String: kSecClassGenericPassword as String,
            kSecAttrAccount as String: key,
            kSecReturnData as String: kCFBooleanTrue!,
            kSecMatchLimit as String: kSecMatchLimitOne ] as [String : Any]

        var item: AnyObject?
        let status = withUnsafeMutablePointer(to: &item) {
            SecItemCopyMatching(query as CFDictionary, UnsafeMutablePointer($0))
        }

        if status == noErr {
            if let data = item as? Data {
                return String(data: data, encoding: .utf8)
            }
        }

        return nil
    }
}

@_cdecl("SpirareBrowserEncryptedPrefs_KeychainHelper_Save")
public func KeychainHelper_Save(key: UnsafePointer<CChar>, data: UnsafePointer<CChar>) -> Bool
{
    return KeychainHelper.save(key: String(cString: key), data: String(cString: data))
}

@_cdecl("SpirareBrowserEncryptedPrefs_KeychainHelper_Load")
public func KeychainHelper_Load(key: UnsafePointer<CChar>, ptr: UnsafeMutablePointer<UnsafePointer<CChar>?>, length: UnsafeMutablePointer<Int32>)
{
    let valueString = KeychainHelper.load(key: String(cString: key))

    if let validString = valueString {
        validString.withCString { cString in
            let lengthOfCString = strlen(cString)
            let buffer = UnsafeMutablePointer<CChar>.allocate(capacity: lengthOfCString + 1)
            buffer.initialize(from: cString, count: lengthOfCString + 1)
            ptr.pointee = UnsafePointer(buffer)
            length.pointee = Int32(lengthOfCString)
        }
    } else {
        ptr.pointee = nil
        length.pointee = 0
    }
}

@_cdecl("SpirareBrowserEncryptedPrefs_DeallocatePointer")
public func DeallocatePointer(ptr: UnsafeMutablePointer<CChar>?) {
    ptr?.deallocate()
}
