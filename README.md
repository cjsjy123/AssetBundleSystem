# AssetBundleSystem

# 介绍:

​	这是一个简单data-system 结构的Assetbundle 加载系统，同时附带有相应的工具界面，方便调整assetbundle,并对当前运行时的情况proifer，通过相关的build 工具build assetbundle 和 dep file.

### 模块：

#### 	ParseAssetInfoSystem:

​	  **解析depfile(assetbundle 信息文件.)的system，它会首先尝试从persistent 路径尝试加载depfile，如果失败了，则会从streamassetpath 路径加载 depfile，都失败了，会返回errcode -1。**

​	解析depfile会使用DefaultParseDepReader ，这个继承了IParseDependencyReader （你可以自行实现相关的数据序列化解析器，然后在AssetBundleTypeGetter 中进行替换，如果一旦和editor中的DefaultParseDepWriter 序列化器出入过大，则对editor 的相关部分也进行相应的替换即可）

#### 	ParseRemoteSystem：

​	**远程版本文件的解析system。最好自行实现相关的逻辑，默认情况下，会下载远程的depfile作为版本对比文件。逻辑实现大致为两个方向，**

​	1.把所有文件的md5或者其他的uniqueId的数据集合文件下载下来，然后全部存储到一个容器中，然后对本地相关文件进行对比，然后得到一个list，然后把所有在list内的新文件下载下来。 

​	2.远程服务器那边已经做好了相关的更新统计，然后把相关的资源打成了zip包，那么客户端只需要对比版本号就可以了，然后下载新的文件替换本地的。

​	相关的接口为IRemoteAsset

#### 	LoadCommonObjectSystem:

​	**加载assetbundle /asset的加载system。相关的根据资源情况有几个派生。**

​	1.asset 类型的资源，一般可能比如是一些text /texture /font 等等类似的资源，需要赋值或者进行解析。一些比如text的数据可能用完就dispose了的，则会自动释放相关资源， 一些可能需要被一些其他对象引用 的资源，则需要进行添加引用的操作。

​	2.gameobject 类型的资源，通常是一些prefab类型的资源，需要进行实例化。实例化的时候调用resultargs的相关接口，其会自动为此附加引用对象的管理，如果不进行实例化的操作，则会因为没有引用，而在接下来的某个时间被destory，如有此种资源，建议使用preload的相关接口。

​	3.scene类型的资源，无需做太多处理，主要操作在resultargs 中，通过SceneTrackingSystem进行场景的监听。

#### 	AssetBundleDownloadSystem：

​	**资源下载system，通过内置的IDownloader实现的简单下载器进行下载。可以自行实现相关的下载器在AssetBundleTypeGetter中进行替换，否则使用DefualtDownloader**

#### 	**LoadDispatcherSystem**：

​	**加载任务派发处理的system，这里会进行相关任务的invoke callback /add reference / auto instantiate等相关的操作.**	

#### 	AssetBundleMemorySystem:

​	**Assetbundle的内存管理system，对使用中的assetbundle 判断使用情况，决定是否需要被卸载，一旦判定为卸载资源，则会使用Unload(true) 并且Destory Loaded Objects,进行卸载。**

#### 	SceneTrackingSystem：

​	**场景监听system。**

#### 	LoadProfilerSystem：

​	**为profiler 模块服务的对runtime的一些assetbundle/task情况进行记录的system.**

#### 	RemoveTaskSystem:

​	**根据用户操作，进行相应的task移除的system.*

## 接口说明：

 #### AssetBundleLoadManager：

​	LoadAsset:加载资源

​	LoadAssetAsync：异步加载资源.

​	PreLoadAsset:预加载资源

​	PreLoadAssetAsync：异步预加载 

​	LoadGameObject:加载gameobject

​	LoadGameObjectAsync:异步加载gameobject

​	ReLoad:更新了资源信息，会对整个system进行重入操作.

​	UnloadAllUnusedAssets:卸载未使用的资源.

​	IsProfilering:判断是否profilering

​	SetProfiler：设置profiler状态



LoadReturnArgs:

​	SetActive:设置是否激活，对scene/gameobject都有效

​	SetCallBack：设置回调函数

​	SetFreeSize: 加载的system会通过config中的TaskLoadAssetLimit 限制来管控当前帧的负担，如果过重，则会放入到下一帧进行处理，如果TaskLoadAssetLimit 过大则可能会导致卡顿的情况产生。一旦Freesize设置为true则会忽略相关的管控.

