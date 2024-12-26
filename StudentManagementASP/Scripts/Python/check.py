import cv2
import sys
import json
import numpy as np

def check_face_position(image_path):
    # Đọc ảnh
    image = cv2.imread(image_path)
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

    # Load cascade nhận diện khuôn mặt
    face_cascade = cv2.CascadeClassifier(cv2.data.haarcascades + 'haarcascade_frontalface_default.xml')

    # Phát hiện khuôn mặt
    faces = face_cascade.detectMultiScale(gray, scaleFactor=1.1, minNeighbors=5, minSize=(30, 30))

    if len(faces) == 0:
        return False

    # Lấy tọa độ khuôn mặt đầu tiên
    (x, y, w, h) = faces[0]

    # Tọa độ trung tâm khuôn mặt
    face_center = (x + w // 2, y + h // 2)

    # Tọa độ trung tâm màn hình
    img_height, img_width = image.shape[:2]
    screen_center = (img_width // 2, img_height // 2)

    # Kiểm tra khoảng cách
    distance = np.sqrt((face_center[0] - screen_center[0]) ** 2 + (face_center[1] - screen_center[1]) ** 2)
    return distance < 50  # Khoảng cách tối đa

if __name__ == "__main__":
    image_path = sys.argv[1]
    result = check_face_position(image_path)

    # Đảm bảo result là kiểu bool chuẩn của Python
    result = bool(result)
    
    print(json.dumps(result))
