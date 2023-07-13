using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ILOG.Concert;
using ILOG.CPLEX;
using System.Collections;

namespace OR
{
    class Program
    {
        static void Main(string[] args)
        {

            #region(讀工廠數量)
            //open csv file
            StreamReader Set_i = new StreamReader(@"Data\factory.csv");

            // Read csv to string I
            List<string> I = new List<string>();
            string Set_line;
            while ((Set_line = Set_i.ReadLine()) != null)
            {
                I.Add(Set_line); //一次讀一行，存入List I中
            }
            Set_i.Close();
            #endregion
            #region(讀產品的數量)
            //open csv file
            StreamReader Set_j = new StreamReader(@"Data\product.csv");

            // Read csv to string J
            List<string> J = new List<string>();
            while ((Set_line = Set_j.ReadLine()) != null)
            {
                J.Add(Set_line); //一次讀一行，存入List J中
            }
            Set_j.Close();
            #endregion
            #region(讀需求的週期長度)
            //open csv file
            StreamReader Set_t = new StreamReader(@"Data\period.csv");

            // Read csv to string T
            List<string> T = new List<string>();
            while ((Set_line = Set_t.ReadLine()) != null)
            {
                T.Add(Set_line);
            }
            Set_t.Close();

            //print list T 中的內容可以幫助檢查讀入的資料是否正確
            //foreach (string a in I)
            //{
            //    Console.WriteLine(a);
            //}
            #endregion
            #region(讀需求量)
            //open csv
            StreamReader Parameter_file = new StreamReader(@"Data\demand.csv");

            //Create two dimensions array
            double[][] demand = new double[J.Count][]; //第一維的大小是產品數量
            for (int i = 0; i < J.Count; i++)
            {
                demand[i] = new double[T.Count]; // 第二維的大小是週期長度
            }

            //read csv to 2D array
            string Parameter_line;
            while ((Parameter_line = Parameter_file.ReadLine()) != null)
            {
                string[] temp_line = Parameter_line.Split(','); //一次讀一行，以逗號隔開數字(j,t,djt)
                //將index轉成int, 需求轉成double
                demand[int.Parse(temp_line[0]) - 1][int.Parse(temp_line[1]) - 1] = Convert.ToDouble(temp_line[2]);
            }

            //檢查讀入的資料是否正確
            //Console.WriteLine(demand[10][2]); //第11個產品在第3期的需求
            #endregion
            #region(輸出csv檔案)
            //create csv file
            StreamWriter sales = new StreamWriter(@"result_sales.csv");
            StreamWriter prod = new StreamWriter(@"result_production.csv");
            StreamWriter bg = new StreamWriter(@"result_backlog.csv");
            StreamWriter Inven= new StreamWriter(@"result_inventory.csv");
            StreamWriter cap = new StreamWriter(@"result_capacity.csv");

            string[] revenue = File.ReadAllLines(@"Data\revenue.csv");
            double[] REV = new double[3412];
            for (int i = 0; i < J.Count; i++)
                REV[i] = Convert.ToDouble(revenue[i]);

            string[] cap1 = File.ReadAllLines(@"Data\CAP.csv");
            double[] CAP = new double[6];
            for (int i = 0; i < I.Count; i++)
                CAP[i] = Convert.ToDouble(cap1[i]);

            string[] hc = File.ReadAllLines(@"Data\HC.csv");
            double[] HC = new double[3412];
            for (int i = 0; i < J.Count; i++)
                HC[i] = Convert.ToDouble(hc[i]);

            string[] uc = File.ReadAllLines(@"Data\UC.csv");
            double[] UC = new double[3412];
            for (int i = 0; i < J.Count; i++)
                UC[i] = Convert.ToDouble(uc[i]);

            string[] pc = File.ReadAllLines(@"Data\PC.csv");
            double[] PC = new double[3412];
            for (int i = 0; i < J.Count; i++)
                PC[i] = Convert.ToDouble(pc[i]);

            StreamReader e = new StreamReader(@"Data\E.csv");
            double[][] E = new double[J.Count][];
            for (int j = 0; j < J.Count; j++)
            {
                E[j] = new double[I.Count]; // 第二維的大小是週期長度
            }
            string Parameter_e;
            int c = 0;
            while ((Parameter_e = e.ReadLine()) != null)
            {
                string[] temp_e = Parameter_e.Split(','); //一次讀一行，以逗號隔開數字(j,t,djt)
                //將index轉成int, 需求轉成double
                for(int i=0;i<I.Count;i++)
                {
                    E[c][i] = Convert.ToDouble(temp_e[i]);
                }
                c++;
                
            }
            for (int j = 0; j < J.Count; j++)
            {
                for (int i = 0; i < I.Count; i++)
                {
                    Console.WriteLine(E[j][i]);
                }
            }
            
            //建立變數
            Cplex cplexModel = new Cplex();
            INumVar[][][] x_ijt = new INumVar[I.Count][][];
            for (int i = 0; i < I.Count; i++)
            {
                x_ijt[i] = new INumVar[J.Count][];
                for (int j = 0; j < J.Count; j++)
                {
                    x_ijt[i][j] = cplexModel.NumVarArray(T.Count, 0.0, double.MaxValue);
                }
            }
            INumVar[][] s_jt = new INumVar[J.Count][];
            for (int j = 0; j < J.Count; j++)
            {
                s_jt[j] = cplexModel.NumVarArray(T.Count, 0.0, double.MaxValue);
            }
            INumVar[][] b_jt = new INumVar[J.Count][];
            for (int j = 0; j < J.Count; j++)
            {
                b_jt[j] = cplexModel.NumVarArray(T.Count, 0.0, double.MaxValue);
            }
            INumVar[][] I_jt = new INumVar[J.Count][];
            for (int j = 0; j < J.Count; j++)
            {
                I_jt[j] = cplexModel.NumVarArray(T.Count, 0.0, double.MaxValue);
            }


            //建目標函數
            ILinearNumExpr TotalPC = cplexModel.LinearNumExpr();
            ILinearNumExpr TotalRev = cplexModel.LinearNumExpr();
            ILinearNumExpr TotalHC = cplexModel.LinearNumExpr();
            ILinearNumExpr TotalUC = cplexModel.LinearNumExpr();

            for (int j=0;j<J.Count;j++)
            {
                for(int t=0;t<T.Count;t++)
                {
                    for(int i = 0; i < I.Count; i++)
                    {
                        TotalPC.AddTerm(x_ijt[i][j][t], (-1)*PC[j]);
                    }
                    TotalRev.AddTerm(s_jt[j][t], REV[j]);
                    TotalHC.AddTerm(I_jt[j][t], (-1) * HC[j]);
                    TotalUC.AddTerm(b_jt[j][t], (-1) * UC[j]);
                }
            }
            cplexModel.AddMaximize(cplexModel.Diff(TotalRev, cplexModel.Sum(TotalPC, TotalHC, TotalUC)));

            //建立限制式
            //限制式1
            int n = 1;
            for (int i = 0; i < I.Count; i++)
            {
                ILinearNumExpr ProdQty = cplexModel.LinearNumExpr();
                for (int j = 0; j < J.Count; j++)
                {
                    for (int t = 0; t < T.Count; t++)
                    {
                        if(E[j][i]==1)
                            ProdQty.AddTerm(x_ijt[i][j][t], 1);
                    }
                }
                cplexModel.AddLe(ProdQty, CAP[i], "C1" + n);
                n++;
            }
            //限制式2
            n = 1;
            for (int i = 0; i < I.Count; i++)
            {
                for (int j = 0; j < J.Count; j++)
                {
                    if (E[j][i] == 1)
                    {
                        ILinearNumExpr TotalXijt1 = cplexModel.LinearNumExpr();
                        ILinearNumExpr Inventory1 = cplexModel.LinearNumExpr();
                        ILinearNumExpr sale = cplexModel.LinearNumExpr();
                        for (int t = 0; t < T.Count; t++)
                        {
                            //TotalXijt1.AddTerm(x_ijt[i][j][t], 1);
                            Inventory1.AddTerm(I_jt[j][t], -1);
                            sale.AddTerm(s_jt[j][t], 1);
                            cplexModel.AddLe(cplexModel.Sum(sale, Inventory1), TotalXijt1, "C2" + n);
                            //cplexModel.AddLe(cplexModel.Sum(s_jt[j][t], cplexModel.Prod(I_jt[j][t], -1)), x_ijt[i][j][t], "C2" + n);
                            n++;
                        }
                    }
                }
            }
            //限制式3
            n = 1;
            for (int j = 0; j <J.Count; j++)
            {
                for (int t = 1; t < T.Count; t++)
                {
                    ILinearNumExpr Inventory = cplexModel.LinearNumExpr();
                    ILinearNumExpr TotalXijt = cplexModel.LinearNumExpr();
                    for (int i = 0; i < I.Count; i++)
                    {
                        if (E[j][i] == 1)
                            TotalXijt.AddTerm(x_ijt[i][j][t], 1);
                    }
                    Inventory.AddTerm(I_jt[j][t], -1);
                    Inventory.AddTerm(I_jt[j][t - 1], 1);
                    Inventory.AddTerm(b_jt[j][t - 1], -1);
                    Inventory.AddTerm(b_jt[j][t], 1);
                    cplexModel.AddEq(cplexModel.Sum(Inventory, TotalXijt), demand[j][t], "C3" + n);
                    //cplexModel.AddEq(cplexModel.Sum(TotalXijt, cplexModel.Prod(I_jt[j][t], -1), I_jt[j][t - 1], cplexModel.Prod(b_jt[j][t - 1], -1), b_jt[j][t]), demand[j][t], "C3" + n);
                    n++;
                }
            }


            /*if (cplexModel.Solve())
            {
                System.Console.WriteLine("Solution status = " + cplexModel.GetStatus());
                System.Console.WriteLine();
                System.Console.WriteLine("Total Profit = " + cplexModel.ObjValue);

                IEnumerator matrixEnum = cplexModel.GetLPMatrixEnumerator();
                matrixEnum.MoveNext();

                ILPMatrix lp = (ILPMatrix)matrixEnum.Current;
            }
            cplexModel.End();*/

            cplexModel.Solve();

            //最佳解
            Console.WriteLine("MAX profit = " + cplexModel.GetObjValue());
            //輸出結果
            double[] Xijt = new double[T.Count];
            double[] Sjt = new double[T.Count];
            double[] Bjt = new double[T.Count];
            double[] ljt = new double[T.Count];
            double[][] capacity = new double[I.Count][];
            for (int i = 0; i < I.Count; i++)
            {
                capacity[i] = new double[T.Count];
            }

            for (int t = 1; t <= T.Count; t++)//在csv檔中第一行寫入時間T=1~24
            {
                sales.Write("T=" + t + ",");
                bg.Write("T=" + t + ",");
                Inven.Write("T=" + t + ",");
                cap.Write("T=" + t + ",");
            }
            sales.WriteLine();
            bg.WriteLine();
            Inven.WriteLine();
            cap.WriteLine();

            //print production
            prod.Write("i,j,t,Xijt");
            prod.WriteLine();
            for (int j = 0; j < J.Count; j++)
            {
                for (int i = 0; i < I.Count; i++)
                {
                    Xijt = cplexModel.GetValues(x_ijt[i][j]);//第j個產品的生產計劃
                    for (int b = 0; b < Xijt.Length; b++)
                    {
                        if (Xijt[b] != 0)
                        {
                            prod.Write("{0},{1},{2},{3}", i + 1, j + 1, b + 1, Xijt[b]);
                            prod.WriteLine();
                            capacity[i][b] = capacity[i][b] + Xijt[b];//計算總產量
                        }
                    }
                }
            }

            //close the file
            sales.Close();
            prod.Close();
            Inven.Close();
            bg.Close();
            cap.Close();
            #endregion

            Console.Read();
            cplexModel.End();
        }
    }
}
