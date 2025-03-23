// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

package org.eframework.u3d.util;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.zip.*;
import android.content.res.AssetFileDescriptor;

public class XFile {
    public interface IZipListener {
        void OnComplete();

        void OnProgress(float progress);

        void OnError(String error);
    }

    public static boolean HasAsset(String file){
        if (file.startsWith("jar:file://")) {
            String asset = file.substring(file.lastIndexOf("/") + 1);
            try {
                com.unity3d.player.UnityPlayer.currentActivity.getAssets().open(asset).close();
                return true;
            } catch (IOException e) {
                return false;
            }
        } else {
            return false;
        }
    }

    public static long AssetSize(String file) {
        if (file.startsWith("jar:file://")) {
            String asset = file.substring(file.lastIndexOf("/") + 1);
            try {
                AssetFileDescriptor fileDescriptor = com.unity3d.player.UnityPlayer.currentActivity.getAssets().openFd(asset);
                long size = fileDescriptor.getLength();
                fileDescriptor.close();
                return size;
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        return -1;
    }

    public static String OpenAsset(String file) {
        if (file.startsWith("jar:file://")) {
            String asset = file.substring(file.lastIndexOf("/") + 1);
            try (InputStream is = com.unity3d.player.UnityPlayer.currentActivity.getAssets().open(asset);
                BufferedReader reader = new BufferedReader(new InputStreamReader(is, StandardCharsets.UTF_8))) {
                StringBuilder sb = new StringBuilder();
                String line;
                while ((line = reader.readLine()) != null) {
                    sb.append(line).append("\n");
                }
                return sb.toString();
            } catch (IOException e) {
                e.printStackTrace();
                return "";
            }
        } else {
            return "";
        }
    }

    public static void Unzip(String src, String to, final IZipListener listener) {
        new Thread(() -> {
            try {
                InputStream fis;
                long total;
                if (src.startsWith("jar:file://")) {
                    String asset = src.substring(src.lastIndexOf("/") + 1);
                    fis = com.unity3d.player.UnityPlayer.currentActivity.getAssets().open(asset);
                    total = fis.available();
                } else {
                    fis = new FileInputStream(src);
                    total = new File(src).length();
                }
                ZipInputStream zis = new ZipInputStream(fis);
                ZipEntry zip;
                byte[] buffer = new byte[2048];
                while ((zip = zis.getNextEntry()) != null) {
                    String name = zip.getName();
                    if (zip.isDirectory()) {
                        name = name.substring(0, name.length() - 1);
                        String folderName = to + File.separator + name;
                        File folder = new File(folderName);
                        folder.mkdirs();
                    } else {
                        String fileName = to + File.separator + name;
                        File file = new File(fileName);
                        if (!file.exists()) {
                            file.getParentFile().mkdirs();
                            file.createNewFile();
                        }

                        FileOutputStream fout = new FileOutputStream(fileName);
                        BufferedOutputStream bout = new BufferedOutputStream(fout);

                        int read;
                        while ((read = zis.read(buffer)) != -1) {
                            bout.write(buffer, 0, read);
                        }

                        zis.closeEntry();
                        bout.close();
                        fout.close();
                    }
                    if (listener != null) listener.OnProgress(1 - (fis.available() * 1f / total));
                }
                fis.close();
                zis.close();
                if (listener != null) {
                    listener.OnProgress(1);
                    listener.OnComplete();
                }
            } catch (Exception e) {
                e.printStackTrace();
                if (listener != null) listener.OnError(e.getMessage());
            }
        }).start();
    }
}
