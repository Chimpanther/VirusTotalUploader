import json, os, subprocess
prs = json.load(open('/tmp/vt-prs-fresh.json'))
test_prs = [p['number'] for p in prs if p['mergeStateStatus'] == 'DIRTY' and ('test' in p['title'].lower() or '🧪' in p['title'])]

for num in test_prs:
    br = next(p['headRefName'] for p in prs if p['number'] == num)
    print(f"Fixing {num}: {br}")
    wt = f"../.vt-worktrees/fix-{num}"
    if not os.path.exists(wt):
        subprocess.run(['git','worktree','add',wt,f'origin/{br}'])
    
    os.chdir(wt)
    subprocess.run(['git','merge','origin/master','--no-edit'])
    
    st = subprocess.check_output(['git','status','--porcelain'], text=True)
    if 'UU uploader/uploader.sln' in st or 'UU uploader/uploader.Tests/uploader.Tests.csproj' in st or 'AA' in st:
        subprocess.run(['git','checkout','--theirs','uploader/uploader.sln'])
        subprocess.run(['git','add','uploader/uploader.sln'])
        # for csproj, assume ours is the one adding the file
        subprocess.run(['git','checkout','--ours','uploader/uploader.Tests/uploader.Tests.csproj'])
        subprocess.run(['git','add','uploader/uploader.Tests/uploader.Tests.csproj'])
        # for cs files
        for line in st.split('\n'):
            if line.startswith('A') or line.startswith('U'):
                path = line[3:]
                if path.endswith('.cs'):
                    subprocess.run(['git','checkout','--ours',path])
                    subprocess.run(['git','add',path])
        subprocess.run(['git','-c','user.name=Magnus Aune','-c','user.email=dev@chimpanther.com','commit','--no-edit'])
    
    subprocess.run(['git','push','origin',f'HEAD:{br}'])
    os.chdir('/srv/pluto/workspace/VirusTotalUploader')
