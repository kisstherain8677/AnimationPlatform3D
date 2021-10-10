# coding=utf-8

import os
import json


def transStr():
    # 获取目标文件夹的路径
    filedir = os.getcwd()
    # 获取文件夹中的文件名称列表
    filenames = os.listdir(filedir)
    # 遍历文件名
    for filename in filenames:
        filepath = filedir + '/' + filename
        print(filepath)
        if (not filepath.endswith('json')):
            continue

        after = []
        # 打开文件取出数据并修改，然后存入变量
        with open(filepath, 'rb') as f:
            data = json.load(f)
            print(type(data))
            list = data["keyFrames"]
            for zidian in list:
                zidian['action'] = int(zidian['action'])
                zidian['type'] = int(zidian['type'])
                zidian['timestamp'] = float(zidian['timestamp'])

                zidian['startpos'] = transList(zidian['startpos'],10.0)
                zidian['endpos'] = transList(zidian['endpos'],10.0)
                zidian['startrotation'] = transList(zidian['startrotation'])
                zidian['endrotation'] = transList(zidian['endrotation'])
                zidian['startscale'] = transList(zidian['startscale'])
                zidian['endscale'] = transList(zidian['endscale'])

                zidian['duration']=float(zidian['duration'])
                zidian['loop'] = int(zidian['loop'])

                if(zidian['name']=='LoveDuck'):
                    for index in range(len(zidian['startscale'])):
                        zidian['startscale'][index]=0.05
                        zidian['endscale'][index]=0.05

                if (zidian['name'] == 'Helicopter'):
                    for index in range(len(zidian['startscale'])):
                        zidian['startscale'][index] = 3
                        zidian['endscale'][index] = 3


            after = data


        # 打开文件并覆盖写入修改后内容
        with open(filepath, 'wb') as f:
            data = json.dump(after, f)

def transList(templist,factory=1):
    reList=[]
    for index in range(len(templist)):
        reList.append(float(templist[index])*factory)
    return reList


if __name__ == '__main__':
    transStr()
