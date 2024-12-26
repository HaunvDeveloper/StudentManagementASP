import os
import itertools
import random
import numpy as np
from tqdm import tqdm
from sklearn.metrics.pairwise import cosine_similarity
from sklearn.metrics import roc_curve, auc
from deepface import DeepFace
import matplotlib.pyplot as plt

def load_embeddings(users_dir='data/users'):
    """
    Tải tất cả các embeddings từ thư mục người dùng.
    
    Returns:
    - persons (dict): Dictionary mapping user IDs to list of embeddings.
    """
    persons = {}
    user_ids = os.listdir(users_dir)
    for user_id in user_ids:
        user_folder = os.path.join(users_dir, user_id)
        if os.path.isdir(user_folder):
            embeddings = []
            for file in os.listdir(user_folder):
                if file.endswith('.npy'):
                    embedding = np.load(os.path.join(user_folder, file))
                    # Chuyển đổi mảng thành vector 1D nếu cần
                    embedding = embedding.flatten()
                    embeddings.append(embedding)
            persons[user_id] = embeddings
    return persons


def generate_pairs(persons, num_negative_per_person=10):
    """
    Tạo cặp ảnh cùng người và khác người.
    
    Parameters:
    - persons (dict): Dictionary mapping user IDs to list of embeddings.
    - num_negative_per_person (int): Số cặp khác người mỗi người.
    
    Returns:
    - pairs (list): List of tuples (embedding1, embedding2, label)
    """
    pairs = []
    
    # Tạo cặp cùng người (positive pairs)
    for user_id, embeddings in persons.items():
        positive_pairs = list(itertools.combinations(embeddings, 2))
        for pair in positive_pairs:
            pairs.append((pair[0], pair[1], 1))
    
    # Tạo cặp khác người (negative pairs)
    user_ids = list(persons.keys())
    for user_id in user_ids:
        other_users = [uid for uid in user_ids if uid != user_id]
        if len(other_users) < num_negative_per_person:
            negative_users = other_users
        else:
            negative_users = random.sample(other_users, num_negative_per_person)
        
        for neg_uid in negative_users:
            embedding1 = random.choice(persons[user_id])
            embedding2 = random.choice(persons[neg_uid])
            pairs.append((embedding1, embedding2, 0))
    
    return pairs

def calculate_optimal_threshold(pairs, save_path='data/embeddings/optimal_threshold.npy'):
    """
    Tính toán và lưu trữ ngưỡng tối ưu.
    
    Parameters:
    - pairs (list): List of tuples (embedding1, embedding2, label)
    - save_path (str): Đường dẫn lưu ngưỡng tối ưu.
    
    Returns:
    - optimal_threshold (float): Ngưỡng tối ưu.
    """
    similarities = []
    labels = []
    
    for emb1, emb2, label in tqdm(pairs, desc="Calculating similarities"):
        sim = cosine_similarity([emb1], [emb2])[0][0]
        similarities.append(sim)
        labels.append(label)
    
    similarities = np.array(similarities)
    labels = np.array(labels)
    
    # Tính ROC và AUC
    fpr, tpr, thresholds = roc_curve(labels, similarities)
    roc_auc = auc(fpr, tpr)
    print(f"AUC: {roc_auc:.4f}")
    
    # Vẽ ROC Curve
    plt.figure(figsize=(8, 6))
    plt.plot(fpr, tpr, color='darkorange', lw=2, label=f'ROC curve (AUC = {roc_auc:.4f})')
    plt.plot([0, 1], [0, 1], color='navy', lw=2, linestyle='--')
    plt.xlim([-0.01, 1.0])
    plt.ylim([0.0, 1.05])
    plt.xlabel('False Positive Rate')
    plt.ylabel('True Positive Rate')
    plt.title('Receiver Operating Characteristic (ROC) Curve')
    plt.legend(loc="lower right")
    plt.grid(True)
    plt.show()
    
    # Tìm ngưỡng tối ưu (Youden's J statistic)
    optimal_idx = np.argmax(tpr - fpr)
    optimal_threshold = thresholds[optimal_idx]
    
    # Làm tròn ngưỡng tối ưu đến 4 chữ số thập phân
    optimal_threshold_rounded = round(optimal_threshold, 4)
    print(f"Optimal Threshold: {optimal_threshold_rounded:.4f}")
    
    # Lưu ngưỡng tối ưu
    os.makedirs(os.path.dirname(save_path), exist_ok=True)
    np.save(save_path, optimal_threshold_rounded)
    
    return optimal_threshold_rounded


if __name__ == "__main__":
    # Tải embeddings từ người dùng
    persons = load_embeddings()
    print(f"Tổng số người dùng: {len(persons)}")
    
    # Tạo cặp ảnh
    pairs = generate_pairs(persons, num_negative_per_person=10)
    print(f"Tổng số cặp đã tạo: {len(pairs)}")
    
    # Tính toán và lưu ngưỡng tối ưu
    optimal_threshold = calculate_optimal_threshold(pairs)
    print(f"Ngưỡng tối ưu đã được lưu tại data/embeddings/optimal_threshold.npy")
