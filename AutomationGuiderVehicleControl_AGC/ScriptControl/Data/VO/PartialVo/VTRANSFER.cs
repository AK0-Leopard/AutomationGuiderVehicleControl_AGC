using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.Data.VO.Interface;
using com.mirle.ibg3k0.sc.ObjectRelay;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc
{
    public partial class VTRANSFER
    {
        public const int MAX_ASSIGNED_TRANSFER_RETURN_TO_QUEUE_TIMES = 3;

        public static ConcurrentDictionary<string, VTRANSFER> VTransferInfoList { get; private set; } = new ConcurrentDictionary<string, VTRANSFER>();
        public Stopwatch CountersownKeepTime;
        public int ReturnToQueueTimes { get; private set; } = 0;
        public static (bool isSuccess, VTRANSFER tranCmd) tryGetTransferByID(string id)
        {
            bool is_get = VTransferInfoList.TryGetValue(SCUtility.Trim(id, true), out var tran);
            return (is_get, tran);
        }

        public static int GetReturnToQueueTimes(string cmd_id)
        {
            if (VTransferInfoList.TryGetValue(cmd_id, out VTRANSFER tran_cmd))
            {
                return tran_cmd.ReturnToQueueTimes;
            }
            return 0;
        }
        public void IncrementReturnToQueueTimes()
        {
            ReturnToQueueTimes++;
        }
        public ACMD ConvertToCmd(BLL.PortStationBLL portStationBLL, BLL.SequenceBLL sequenceBLL, AVEHICLE assignVehicle)
        {
            var source_port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTSOURCE);
            var desc_port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTDESTINATION);
            //如果HostSource port是與車子一樣的話，代表是要去進行Unload的動作
            bool source_is_a_port = portStationBLL.OperateCatch.IsExist(this.HOSTSOURCE);

            string host_source = source_is_a_port ? this.HOSTSOURCE : "";
            E_CMD_TYPE cmd_type = source_is_a_port ? E_CMD_TYPE.LoadUnload : E_CMD_TYPE.Unload; //如果Source不是Port的話，則代表是在車上

            string from_adr = source_port_station == null ? string.Empty : source_port_station.ADR_ID;
            string to_adr = desc_port_station == null ? string.Empty : desc_port_station.ADR_ID;
            return new ACMD()
            {
                ID = sequenceBLL.getCommandID(SCAppConstants.GenOHxCCommandType.Auto),
                TRANSFER_ID = this.ID,
                VH_ID = assignVehicle.VEHICLE_ID,
                CARRIER_ID = this.CARRIER_ID,
                CMD_TYPE = cmd_type,
                SOURCE = from_adr,
                DESTINATION = to_adr,
                PRIORITY = this.PRIORITY_SUM,
                CMD_INSER_TIME = DateTime.Now,
                CMD_STATUS = E_CMD_STATUS.Queue,
                SOURCE_PORT = host_source,
                DESTINATION_PORT = this.HOSTDESTINATION,
                COMPLETE_STATUS = ProtocolFormat.OHTMessage.CompleteStatus.Move
            };
        }


        public ACMD ConvertToCmdForUnloadHPREmptyCST(BLL.PortStationBLL portStationBLL, BLL.SequenceBLL sequenceBLL, AVEHICLE assignVehicle)
        {
            //var source_port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTSOURCE);
            var desc_port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTDESTINATION);

            //如果HostSource port是與車子一樣的話，代表是要去進行Unload的動作
            bool source_is_a_port = false;

            string host_source = source_is_a_port ? this.HOSTSOURCE : "";
            E_CMD_TYPE cmd_type = E_CMD_TYPE.Load; //目的地為虛擬Port的命令要轉換為Load命令給AGV

            //string from_adr = source_port_station == null ? string.Empty : source_port_station.ADR_ID;
            string from_adr = string.Empty;
            string to_adr = desc_port_station == null ? string.Empty : desc_port_station.ADR_ID;
            return new ACMD()
            {
                ID = sequenceBLL.getCommandID(SCAppConstants.GenOHxCCommandType.Auto),
                TRANSFER_ID = this.ID,
                VH_ID = assignVehicle.VEHICLE_ID,
                CARRIER_ID = this.CARRIER_ID,
                CMD_TYPE = cmd_type,
                SOURCE = from_adr,
                DESTINATION = to_adr,
                PRIORITY = this.PRIORITY_SUM,
                CMD_INSER_TIME = DateTime.Now,
                CMD_STATUS = E_CMD_STATUS.Queue,
                SOURCE_PORT = host_source,
                DESTINATION_PORT = this.HOSTDESTINATION,
                COMPLETE_STATUS = ProtocolFormat.OHTMessage.CompleteStatus.Move
            };
        }
        public ACMD ConvertToCmdForLoadHPREmptyCST(BLL.PortStationBLL portStationBLL, BLL.SequenceBLL sequenceBLL, AVEHICLE assignVehicle)
        {
            var source_port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTSOURCE);
            //var desc_port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTDESTINATION);

            //如果HostSource port是與車子一樣的話，代表是要去進行Unload的動作
            bool source_is_a_port = portStationBLL.OperateCatch.IsExist(this.HOSTSOURCE);

            string host_source = source_is_a_port ? this.HOSTSOURCE : "";
            E_CMD_TYPE cmd_type = E_CMD_TYPE.Load; //目的地為虛擬Port的命令要轉換為Load命令給AGV

            string from_adr = source_port_station == null ? string.Empty : source_port_station.ADR_ID;
            string to_adr = string.Empty;
            return new ACMD()
            {
                ID = sequenceBLL.getCommandID(SCAppConstants.GenOHxCCommandType.Auto),
                TRANSFER_ID = this.ID,
                VH_ID = assignVehicle.VEHICLE_ID,
                CARRIER_ID = this.CARRIER_ID,
                CMD_TYPE = cmd_type,
                SOURCE = from_adr,
                DESTINATION = to_adr,
                PRIORITY = this.PRIORITY_SUM,
                CMD_INSER_TIME = DateTime.Now,
                CMD_STATUS = E_CMD_STATUS.Queue,
                SOURCE_PORT = host_source,
                DESTINATION_PORT = string.Empty,
                COMPLETE_STATUS = ProtocolFormat.OHTMessage.CompleteStatus.Move
            };
        }
        public string getSourcePortGroupID(BLL.PortStationBLL portStationBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTSOURCE);
            if (port_station == null) return "";
            return SCUtility.Trim(port_station.GROUP_ID, true);
        }
        public string getSourcePortEQID(BLL.PortStationBLL portStationBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTSOURCE);
            if (port_station == null) return "";
            return SCUtility.Trim(port_station.EQPT_ID, true);
        }
        public AEQPT getSourcePortEQ(BLL.EqptBLL eqptBLL)
        {
            var eq = eqptBLL.OperateCatch.GetEqpt(this.HOSTSOURCE);
            if (eq == null) return null;
            return eq;
        }


        public string getSourcePortNodeID(BLL.PortStationBLL portStationBLL, BLL.EqptBLL eqptBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTSOURCE);
            if (port_station == null) return "";
            var eq = port_station.GetEqpt(eqptBLL);
            if (eq == null) return "";
            return SCUtility.Trim(eq.NODE_ID, true);
        }

        public string getTragetPortEQID(BLL.PortStationBLL portStationBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTDESTINATION);
            if (port_station == null) return "";
            return SCUtility.Trim(port_station.EQPT_ID, true);
        }

        public AEQPT getTragetPortEQ(BLL.PortStationBLL portStationBLL, BLL.EqptBLL eqptBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTDESTINATION);
            if (port_station == null) return null;
            return port_station.GetEqpt(eqptBLL);
        }


        public string getTragetPortNodeID(BLL.PortStationBLL portStationBLL, BLL.EqptBLL eqptBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTDESTINATION);
            if (port_station == null) return "";
            var eq = port_station.GetEqpt(eqptBLL);
            if (eq == null) return "";
            return SCUtility.Trim(eq.NODE_ID, true);
        }
        public AEQPT getTragetPortEQ(BLL.EqptBLL eqptBLL)
        {
            var eq = eqptBLL.OperateCatch.GetEqpt(this.HOSTDESTINATION);
            if (eq == null) return null;
            return eq;
        }

        public bool IsTargetPortAGVStation(BLL.EqptBLL eqptBLL)
        {
            var eq = eqptBLL.OperateCatch.GetEqpt(this.HOSTDESTINATION);
            if (eq == null) return false;
            return eq is IAGVStationType;
        }

        public bool IsSourceOnVh(BLL.VehicleBLL vehicleBLL)
        {
            var vh = vehicleBLL.cache.getVehicleByLocationRealID(HOSTSOURCE);
            return vh != null;
        }

        public BLL.CMDBLL.CommandTranDir GetTransferDir()
        {
            return BLL.CMDBLL.GetTransferDir(this);
        }

        public bool IsExcuteTimeOut
        {
            get
            {
                bool is_timeout = (TRANSFERSTATE >= E_TRAN_STATUS.Queue && TRANSFERSTATE <= E_TRAN_STATUS.Canceled) &&
                                    DateTime.Now > CMD_INSER_TIME.AddMilliseconds(SystemParameter.TransferCommandExcuteTimeOut_mSec);
                return is_timeout;
            }
        }
        public bool isLoading
        {
            get
            {
                COMMANDSTATE = COMMANDSTATE & 252;
                return COMMANDSTATE == ATRANSFER.COMMAND_STATUS_BIT_INDEX_LOADING;
            }
        }
        public bool isUnloading
        {

            get
            {
                COMMANDSTATE = COMMANDSTATE & 224;
                return COMMANDSTATE == ATRANSFER.COMMAND_STATUS_BIT_INDEX_UNLOADING;
            }
        }
        public bool IsOnVh(BLL.VehicleBLL vehicleBLL)
        {
            var vh_obj = vehicleBLL.cache.getVehicleByRealID(SCUtility.Trim(HOSTSOURCE, true));
            return vh_obj != null;
        }


        public override string ToString()
        {
            return $"Command:{this.ID},source:{this.HOSTSOURCE},desc:{this.HOSTDESTINATION},inser time:{CMD_INSER_TIME.ToString()}";
        }

        public bool IsCrossZoneTransfer(BLL.PortStationBLL portStationBLL, BLL.ZoneBLL zoneBLL)
        {
            var source_zone = getSourcePortZone(portStationBLL, zoneBLL);
            var target_zone = getTragetPortZone(portStationBLL, zoneBLL);
            if (source_zone != target_zone)
            {
                return true;
            }
            return false;
        }
        private AZONE getSourcePortZone(BLL.PortStationBLL portStationBLL, BLL.ZoneBLL zoneBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTSOURCE);
            if (port_station == null) return null;
            return port_station.GetZone(zoneBLL);
        }
        private AZONE getTragetPortZone(BLL.PortStationBLL portStationBLL, BLL.ZoneBLL zoneBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTDESTINATION);
            if (port_station == null) return null;
            return port_station.GetZone(zoneBLL);
        }
        public (bool isSpecify, string vhID) CheckIsSpecifyVhToChrage(BLL.VehicleBLL vehicleBLL)
        {
            string source_port = SCUtility.Trim(HOSTSOURCE, true);
            string dest_port = SCUtility.Trim(HOSTDESTINATION, true);
            if (!SCUtility.isMatche(source_port, dest_port))
            {
                return (false, "");
            }
            if (source_port.Length < 2)
            {
                return (false, "");
            }
            if (dest_port.Length < 2)
            {
                return (false, "");
            }
            string sub_source_port_id = source_port.Substring(source_port.Length - 2, 2);
            string sub_dest_port_id = dest_port.Substring(source_port.Length - 2, 2);

            bool is_vh_source = vehicleBLL.cache.getVehicleByRealID(sub_source_port_id) != null;
            bool is_vh_dest = vehicleBLL.cache.getVehicleByRealID(sub_dest_port_id) != null;
            if (!is_vh_source || !is_vh_dest)
            {
                return (false, "");
            }
            return (true, sub_dest_port_id);
        }

        internal bool Put(VTRANSFER current_cmd)
        {
            SCUtility.TrimAllParameter(current_cmd);
            bool is_change = false;
            if (!SCUtility.isMatche(ID, current_cmd.ID))
            {
                ID = current_cmd.ID;
                is_change = true;
            }
            if (!SCUtility.isMatche(LOT_ID, current_cmd.LOT_ID))
            {
                LOT_ID = current_cmd.LOT_ID;
                is_change = true;
            }
            if (!SCUtility.isMatche(CARRIER_ID, current_cmd.CARRIER_ID))
            {
                CARRIER_ID = current_cmd.CARRIER_ID;
                is_change = true;
            }
            if (TRANSFERSTATE != current_cmd.TRANSFERSTATE)
            {
                TRANSFERSTATE = current_cmd.TRANSFERSTATE;
                is_change = true;
            }
            if (COMMANDSTATE != current_cmd.COMMANDSTATE)
            {
                COMMANDSTATE = current_cmd.COMMANDSTATE;
                is_change = true;
            }
            if (!SCUtility.isMatche(HOSTSOURCE, current_cmd.HOSTSOURCE))
            {
                HOSTSOURCE = current_cmd.HOSTSOURCE;
                is_change = true;
            }
            if (!SCUtility.isMatche(HOSTDESTINATION, current_cmd.HOSTDESTINATION))
            {
                HOSTDESTINATION = current_cmd.HOSTDESTINATION;
                is_change = true;
            }
            if (PRIORITY != current_cmd.PRIORITY)
            {
                PRIORITY = current_cmd.PRIORITY;
                is_change = true;
            }
            if (CHECKCODE != current_cmd.CHECKCODE)
            {
                CHECKCODE = current_cmd.CHECKCODE;
                is_change = true;
            }
            if (CMD_INSER_TIME != current_cmd.CMD_INSER_TIME)
            {
                CMD_INSER_TIME = current_cmd.CMD_INSER_TIME;
                is_change = true;
            }
            if (CMD_START_TIME != current_cmd.CMD_START_TIME)
            {
                CMD_START_TIME = current_cmd.CMD_START_TIME;
                is_change = true;
            }
            if (CMD_FINISH_TIME != current_cmd.CMD_FINISH_TIME)
            {
                CMD_FINISH_TIME = current_cmd.CMD_FINISH_TIME;
                is_change = true;
            }
            if (TIME_PRIORITY != current_cmd.TIME_PRIORITY)
            {
                TIME_PRIORITY = current_cmd.TIME_PRIORITY;
                is_change = true;
            }
            if (PORT_PRIORITY != current_cmd.PORT_PRIORITY)
            {
                PORT_PRIORITY = current_cmd.PORT_PRIORITY;
                is_change = true;
            }
            if (REPLACE != current_cmd.REPLACE)
            {
                REPLACE = current_cmd.REPLACE;
                is_change = true;
            }
            if (PRIORITY_SUM != current_cmd.PRIORITY_SUM)
            {
                PRIORITY_SUM = current_cmd.PRIORITY_SUM;
                is_change = true;
            }
            if (RESULT_CODE != current_cmd.RESULT_CODE)
            {
                RESULT_CODE = current_cmd.RESULT_CODE;
                is_change = true;
            }
            if (!SCUtility.isMatche(EXCUTE_CMD_ID, current_cmd.EXCUTE_CMD_ID))
            {
                EXCUTE_CMD_ID = current_cmd.EXCUTE_CMD_ID;
                is_change = true;
            }
            if (CARRIER_INSER_TIME != current_cmd.CARRIER_INSER_TIME)
            {
                CARRIER_INSER_TIME = current_cmd.CARRIER_INSER_TIME;
                is_change = true;
            }
            if (!SCUtility.isMatche(CARRIER_LOCATION, current_cmd.CARRIER_LOCATION))
            {
                CARRIER_LOCATION = current_cmd.CARRIER_LOCATION;
                is_change = true;
            }
            if (CARRIER_INSTALLED_TIME != current_cmd.CARRIER_INSTALLED_TIME)
            {
                CARRIER_INSTALLED_TIME = current_cmd.CARRIER_INSTALLED_TIME;
                is_change = true;
            }
            if (CARRIER_READ_STATUS != current_cmd.CARRIER_READ_STATUS)
            {
                CARRIER_READ_STATUS = current_cmd.CARRIER_READ_STATUS;
                is_change = true;
            }
            if (VH_ID != current_cmd.VH_ID &&
                !SCUtility.isMatche(VH_ID, current_cmd.VH_ID))
            {
                VH_ID = current_cmd.VH_ID;
                is_change = true;
            }
            if (COMPLETE_STATUS != current_cmd.COMPLETE_STATUS)
            {
                COMPLETE_STATUS = current_cmd.COMPLETE_STATUS;
                is_change = true;
            }
            if (!SCUtility.isMatche(PAUSEFLAG, current_cmd.PAUSEFLAG))
            {
                PAUSEFLAG = current_cmd.PAUSEFLAG;
                is_change = true;
            }

            return is_change;
        }
    }

}
