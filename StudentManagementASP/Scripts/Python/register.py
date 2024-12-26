import os
import json
import numpy as np
from deepface import DeepFace

def register_user(user_id, num_images=5):
    """
    Đăng ký người dùng và lưu embeddings khuôn mặt của họ.

    Parameters:
    - user_id (str): Tên hoặc ID của người dùng.
    - num_images (int): Số lượng ảnh cần chụp.
    """
    try:
        # Create user folder
        user_folder = os.path.join('Scripts', 'Data', 'users', str(user_id))
        os.makedirs(user_folder, exist_ok=True)

        # Temporary save directory
        temp_save_dir = os.path.join('wwwroot', 'TempImages', user_id)
        
        os.makedirs(temp_save_dir, exist_ok=True)

        embeddings_saved = []

        # Process images in the temp_save_dir
        for file in os.listdir(temp_save_dir):
            if file.endswith('.jpg') or file.endswith('.png'):
                img_path = os.path.join(temp_save_dir, file)

                try:
                    # Extract embedding using DeepFace
                    embedding = DeepFace.represent(img_path=img_path, model_name='Facenet512', enforce_detection=False)[0]['embedding']
                    embedding = np.array(embedding).flatten()

                    # Save embedding to user folder
                    embedding_file = os.path.join(user_folder, f"{file.split('.')[0]}.npy")
                    np.save(embedding_file, embedding)
                    embeddings_saved.append(embedding_file)

                except Exception as e:
                    return json.dumps({
                        "success": False,
                        "error": f"Error extracting embedding from image {file}: {str(e)}"
                    })

        # Clean up temporary images
        for file in os.listdir(temp_save_dir):
            if file.endswith('.jpg') or file.endswith('.png'):
                os.remove(os.path.join(temp_save_dir, file))

        # Calculate the optimal threshold
        from calculate_threshold import load_embeddings, generate_pairs, calculate_optimal_threshold
        persons = load_embeddings(users_dir=os.path.join('Scripts', 'Data', 'users'))  # Load all user embeddings
        pairs = generate_pairs(persons, num_negative_per_person=10)  # Generate pairs for threshold calculation
        optimal_threshold = calculate_optimal_threshold(pairs)  # Calculate optimal threshold

        # Return success response
        return "successCode:ABCDE"

    except Exception as e:
        return json.dumps({
            "success": False,
            "error": str(e)
        })

if __name__ == "__main__":
    os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2'  # Suppress TensorFlow logs
    import sys
    if len(sys.argv) < 2:
        print(json.dumps({"success": False, "error": "Missing user_id argument"}))
        sys.exit(1)

    user_id = sys.argv[1]
    result = register_user(user_id, num_images=5)
    print(result)
