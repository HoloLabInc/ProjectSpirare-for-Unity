package jp.co.hololab.spirare.browser.encryptedprefs;

import android.content.Context;
import android.content.SharedPreferences;
import androidx.security.crypto.EncryptedSharedPreferences;
import androidx.security.crypto.MasterKey;
import java.io.IOException;
import java.security.GeneralSecurityException;

public class EncryptedPreferencesManager {
    private SharedPreferences sharedPreferences;

    public EncryptedPreferencesManager(Context context, String preferenceName) {
        try {
            MasterKey masterKey = new MasterKey.Builder(context)
                    .setKeyScheme(MasterKey.KeyScheme.AES256_GCM)
                    .build();

            sharedPreferences = EncryptedSharedPreferences.create(
                    context,
                    preferenceName,
                    masterKey,
                    EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV,
                    EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public boolean isAvailable() {
        return sharedPreferences != null;
    }

    public boolean saveString(String key, String value) {
        if (sharedPreferences == null) {
            return false;
        }

        try {
            sharedPreferences.edit().putString(key, value).apply();
            return true;
        } catch (Exception e) {
            e.printStackTrace();
            return false;
        }
    }

    public String loadString(String key) {
        if (sharedPreferences == null) {
            return null;
        }

        try {
            return sharedPreferences.getString(key, null);
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }
}
