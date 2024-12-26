import os
import numpy as np
import pandas as pd
from datetime import datetime
from deepface import DeepFace
from sklearn.metrics.pairwise import cosine_similarity
from tqdm import tqdm
import cv2

def load_optimal_threshold(threshold_path='data/embeddings/optimal_threshold.npy'):
    """
    Tải ngưỡng tối ưu từ file.
    
    Parameters:
    - threshold_path (str): Đường dẫn tới file lưu ngưỡng.
    
    Returns:
    - threshold (float): Ngưỡng tối ưu.
    """
    if os.path.exists(threshold_path):
        threshold = np.load(threshold_path)
        return threshold
    else:
        raise FileNotFoundError("Không tìm thấy file optimal_threshold.npy")

def convert_npy_to_image(npy_path):
    """
    Chuyển đổi tệp .npy thành ảnh RGB nếu cần thiết.
    
    Parameters:
    - npy_path (str): Đường dẫn tới tệp .npy.
    
    Returns:
    - img (numpy array): Ảnh đã được chuyển đổi.
    """
    img_array = np.load(npy_path)
    
    if len(img_array.shape) == 2:  
        
        img = cv2.cvtColor(img_array, cv2.COLOR_GRAY2RGB)
    else:
        img = img_array
    
    if np.max(img) <= 1.0:  
        img = (img * 255).astype(np.uint8)

    return img


def mark_attendance(image_path, users_dir='Scripts/Data/users', threshold=0.5, attendance_file='attendance_records/attendance.csv'):
    """
    Đánh dấu điểm danh dựa trên ảnh đầu vào.
    
    Parameters:
    - image_path (str): Đường dẫn tới ảnh cần điểm danh.
    - users_dir (str): Thư mục chứa người dùng và embeddings.
    - threshold (float): Ngưỡng tối ưu để xác định cùng người.
    - attendance_file (str): Đường dẫn tới file lưu trữ điểm danh.
    
    Returns:
    - recognized_user (str): ID của người được nhận diện, hoặc "Unknown".
    - similarity (float): Điểm tương đồng.
    """
    recognized_user = "Unknown"
    similarity = 0.0
    
    # Tải embeddings của tất cả người dùng
    persons = {}
    user_ids = os.listdir(users_dir)
    for user_id in user_ids:
        user_folder = os.path.join(users_dir, user_id)
        if os.path.isdir(user_folder):
            embeddings = []
            for file in os.listdir(user_folder):
                if file.endswith('.npy'):
                    embedding = np.load(os.path.join(user_folder, file))
                    embedding = embedding.flatten()  
                    embeddings.append(embedding)
            persons[user_id] = embeddings
    
    try:
        
        
        
        # Trích xuất embedding cho ảnh đầu vào bằng DeepFace (đảm bảo sử dụng model 'Facenet512')
        embedding_input = DeepFace.represent(img_path=image_path, model_name='Facenet512', enforce_detection=False)[0]['embedding']
        embedding_input = np.array(embedding_input).flatten()  
    except Exception as e:
        return recognized_user, similarity
    
    max_sim = 0.0
    matched_user = "Unknown"

    for user_id, embeddings in persons.items():
        for emb in embeddings:
            emb = np.array(emb).flatten()  
            sim = cosine_similarity([embedding_input], [emb])[0][0]
            
            if sim > max_sim:
                max_sim = sim
                matched_user = user_id

    if max_sim >= threshold:
        recognized_user = matched_user
        similarity = max_sim
        print(recognized_user)
    else:
        print("None")

    
    # # Ghi nhận điểm danh
    # if recognized_user != "Unknown":
    #     now = datetime.now()
    #     date_time = now.strftime("%Y-%m-%d %H:%M:%S")
    #     record = {"User ID": recognized_user, "Time": date_time}
        
    #     # Tạo thư mục lưu trữ nếu chưa tồn tại
    #     os.makedirs(os.path.dirname(attendance_file), exist_ok=True)
        
    #     if not os.path.exists(attendance_file):
    #         df = pd.DataFrame(columns=["User ID", "Time"])
    #         df.to_csv(attendance_file, index=False)
        
    #     # Thêm dòng mới
    #     df = pd.read_csv(attendance_file)
    #     # Kiểm tra xem người dùng đã điểm danh trong ngày chưa
    #     today = now.strftime("%Y-%m-%d")
    #     if not ((df["User ID"] == recognized_user) & (df["Time"].str.startswith(today))).any():
    #         df = df.append(record, ignore_index=True)
    #         df.to_csv(attendance_file, index=False)
    #         print(f"Đã điểm danh: {recognized_user} tại {date_time}")
    #     else:
    #         print(f"Người dùng {recognized_user} đã được điểm danh hôm nay.")
    # else:
    #     print("Không nhận diện được người dùng.")
    
    return recognized_user, similarity



def perform_attendance(users_dir='Scripts/Data/users', threshold_path='data/embeddings/optimal_threshold.npy', num_images=1):
    """
    Thực hiện chức năng điểm danh bằng cách chụp ảnh từ webcam và xác định người dùng.
    
    Parameters:
    - users_dir (str): Thư mục chứa người dùng và embeddings.
    - threshold_path (str): Đường dẫn tới file lưu ngưỡng tối ưu.
    - num_images (int): Số lượng ảnh cần chụp (mặc định là 1).
    """
    try:
        threshold = load_optimal_threshold(threshold_path)
    except FileNotFoundError as e:
        return
    
    temp_save_dir = os.path.join('Scripts/Data/captured_faces', 'attendance')
    os.makedirs(temp_save_dir, exist_ok=True)
    
    for file in os.listdir(temp_save_dir):
        if file.endswith('.jpg') or file.endswith('.png'):
            img_path = os.path.join(temp_save_dir, file)
            mark_attendance(img_path, users_dir=users_dir, threshold=threshold)
            os.remove(img_path)

if __name__ == "__main__":
    perform_attendance()
