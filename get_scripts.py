import subprocess
import os

def get_all_scripts():
    # 모든 원격 브랜치 목록 가져오기
    branches = subprocess.check_output(['git', 'branch', '-r']).decode('utf-8').splitlines()
    
    script_data = set() # 중복 제거를 위해 set 사용
    
    for branch in branches:
        branch = branch.strip()
        if '->' in branch: continue # HEAD 포인터 제외
        
        # 해당 브랜치의 파일 목록 가져오기 (.py, .sh 파일만 예시)
        files = subprocess.check_output(['git', 'ls-tree', '-r', '--name-only', branch]).decode('utf-8').splitlines()
        
        for f in files:
            if f.endswith(('.py', '.sh')): # 가져오고 싶은 확장자 설정
                filename = os.path.basename(f)
                branch_name = branch.replace('origin/', '')
                script_data.add((filename, branch_name))

    # 노션에 붙여넣기 좋은 마크다운 표 형식으로 저장
    with open('scripts_for_notion.md', 'w', encoding='utf-8') as f:
        f.write("| 스크립트 이름 | 브랜치 위치 |\n")
        f.write("| :--- | :--- |\n")
        for name, br in sorted(script_data):
            f.write(f"| {name} | {br} |\n")

if __name__ == "__main__":
    get_all_scripts()
