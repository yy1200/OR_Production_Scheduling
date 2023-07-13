![image](https://github.com/yy1200/Production_Scheduling/assets/92247082/0f7e5f21-1f11-4b95-91a7-1da237e20860)
![image](https://github.com/yy1200/Production_Scheduling/assets/92247082/dce84253-b953-42f7-aba7-0dcf789b85d0)

# Explanations of the LP model:
For example, constraint (2) ensures that the total production quantity cannot exceed the installed capacity at each factory.

-	Explain the objective function here.
   
    - 總收入是每個產品的售價乘上銷售數量，總支出是產品的生產成本乘生產數量加存貨成本乘上存貨數量加積欠成本乘積欠數量，兩者相減求出最大利潤。

- Explain constraints (1) ~ (7) here.

  (1) 當月的存貨量等於上個月的存貨量加這個月的生產量減這個月的需求量減上個月的延期交貨量加這個月的延期交貨量
  
  (2) 工廠每個月的總生產量不能超過它的最大容量
  
  (3) 從第一個月到當月的總銷售量不能超過從第一個月到當月的總需求量
  
  (4) 這個月的銷售量不能超過目前庫存，也就是上個月的存貨量加這個月的生產量
  
  (5) 初始存貨量為零
  
  (6) 初始延期交貨量為零
  
  (7) 所有變數皆不能為負

# Explanations of the source code:

  首先使用計時模組去計算程式執行所需的秒數，接著分別讀入工廠、產品的數量以及需求的週期長度和需求量，然後我製作了capacity、revenue、holding cost、backlog cost、production cost、eligible的csv檔，除了最後一個是以二維的形式處理外，其他皆是一維的格式去儲存工廠的最大容量，還有3412個產品的收入及成本，而eligible裡放的是3412 x 6的矩陣去篩選該工廠是否能生產這個產品，如果可以就設值為1，若不行就設0，這樣之後在寫限制式時便可以用判斷式或乘法的方式將符合的限制式條列出來。
  
  將資料都準備好後，就開始建立變數，總共有4個決策變數，分別代表每月每個產品在每個工廠生產的數量，是一個6 x 3412 x 24的三維矩陣、每個產品每個月的銷售量，是一個3412 x 24的二維矩陣、每個產品每個月的延期交貨量，是一個3412 x 25的二維矩陣、每個產品每個月的存貨量，是一個3412 x 25的二維矩陣，後兩者多加了延期交貨量及存貨量的初始值，建立維度的同時也設定它的上下界--都要是大於等於0的值。
  
  然後就是建立目標函數，我建立了4個內積的表達式，以用來放在for迴圈內去進行函式的單項添加，將總收入減總支出以求出最大利潤，以下的限制式也使用相同的方法建立。完成後輸出模型並解決，回傳計時結果並印出最佳解，最後輸出csv檔，包括銷售量、生產量、存貨量、延期交貨量及廠房使用量，以知曉產能是否最大化。

# Results and interpretation/analysis about the solution
- Total profit
  
  總收入為88151540，總成本為47047293，最大利潤是41104247
  ![image](https://github.com/yy1200/Production_Scheduling/assets/92247082/6e70b0d8-a769-4818-969e-e6fc156b610d)

  生產成本、存貨成本跟延期交貨成本中以生產成本為主，存貨成本最低，只有第三個月有存貨(從下圖每月存貨量可知)
  ![image](https://github.com/yy1200/Production_Scheduling/assets/92247082/f4a35c89-00fc-4afc-8ca7-0060409dd7fc)

- The production quantity of project j in factory i in month t

  總生產量DIS最高，BCD次之，再來是HV、MIX，最低為GAD
  ![image](https://github.com/yy1200/Production_Scheduling/assets/92247082/2d3d2247-1161-407c-9cc9-3ecae1b4b1a6)

  A工廠以DIS生產最多，B工廠HV，C工廠BCD，D工廠MIX，E工廠DIS，F工廠 MIX
  ![image](https://github.com/yy1200/Production_Scheduling/assets/92247082/ed90fed2-ec1a-4dd2-a2f0-b4ffe6d2ad85)

- The sales quantity of product j in month t

  銷售額BCD最高，HV次之，MIX跟DIS差距不大；銷售量DIS最高，再來是BCD、HV跟MIX
  ![image](https://github.com/yy1200/Production_Scheduling/assets/92247082/691a499e-2530-4aeb-8c3b-3c169ee549d6)

- The backlog quantity of product j in month t

  第三個月最低，延期交貨量為0，且還有庫存，第十二個月最高
  ![image](https://github.com/yy1200/Production_Scheduling/assets/92247082/c002f0c7-b92f-4311-a3ab-92072b4aba16)

- The inventory level of product j in month t

  只有第三個月有庫存，其他都為0，且還有積欠訂單
  ![image](https://github.com/yy1200/Production_Scheduling/assets/92247082/a6011ef3-7498-4562-ba23-bcd5333f4018)
